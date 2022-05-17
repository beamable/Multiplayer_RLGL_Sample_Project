using System.Collections.Generic;
using BeamableExample.RedlightGreenLight.Character;
using Fusion;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace BeamableExample.RedlightGreenLight
{
    /// <summary>
    /// The Weapon class controls how fast a weapon fires, which projectiles it uses
    /// and the start position and direction of projectiles.
    /// </summary>

    public class Weapon : NetworkBehaviour
    {
        [System.Serializable]
        public class CollisionSettings
        {
            public LayerMask hitMask;
            public float knockBackImpulse;
            public float radius = 0.25f;
            public Vector3 offset = Vector3.zero;
            public float coneAngle = 0f;
        }

        [SerializeField] private WeaponElement _weaponElement;
        [SerializeField] private AudioPlayer _audioPlayer;
        [SerializeField] private GameObject _visualObject;
        public Sprite weaponUIIcon;

        [Header("Settings")]
        [SerializeField] private CollisionSettings _collisionSettings;

        [Networked(OnChanged = nameof(OnFireTickChanged))]
        private int fireTick { get; set; }
        public WeaponElement weaponElement => _weaponElement;

        [Networked(OnChanged = nameof(OnHealthChanged))]
        public int health { get; set; }

        private WeaponManager _weaponManager;

        private void Awake()
        {
            SetActive(false);
        }

        public override void Spawned()
        {
            health = weaponElement._baseHealth;

            _weaponManager = GetComponentInParent<WeaponManager>();
        }

        /// <summary>
        /// Control the visual appearance of the weapon. This is controlled by the Player based
        /// on the currently selected weapon, so the boolean parameter is entirely derived from a
        /// networked property (which is why nothing in this class is sync'ed).
        /// </summary>
        /// <param name="show">True if this weapon is currently active and should be visible</param>
        public void SetActive(bool value)
        {
            if (_visualObject != null)
                _visualObject.SetActive(value);

            if (value == true)
                health = weaponElement._baseHealth;
        }

        public bool IsActive()
        {
            if (_visualObject != null)
            {
                return _visualObject.activeInHierarchy;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Fire a weapon.
        /// This is called in direct response to player input, but only on the server.
        /// </summary>
        /// <param name="runner"></param>
        /// <param name="ownerPlayerRef"></param>
        public void Fire(NetworkRunner runner, PlayerRef ownerPlayerRef)
        {
            PlayerCharacter ownerPlayer = PlayerManager.Get(ownerPlayerRef);
            Vector3 source = ownerPlayer.transform.position + ownerPlayer.Visual.transform.forward * _collisionSettings.offset.z;
            source += ownerPlayer.Visual.transform.right * _collisionSettings.offset.x;
            source += ownerPlayer.Visual.transform.up * _collisionSettings.offset.y;
            List<LagCompensatedHit> _areaHits = new List<LagCompensatedHit>();

            int overlaps = runner.LagCompensation.OverlapSphere(source, _collisionSettings.radius, ownerPlayerRef, _areaHits, _collisionSettings.hitMask, HitOptions.IncludePhysX);
            if (overlaps > 0)
            {
                for (int i = 0; i < overlaps; i++)
                {
                    GameObject hitObject = _areaHits[i].GameObject;
                    if (hitObject)
                    {
                        // Lets see if we can damage this object.
                        ICanTakeDamage target = hitObject.GetComponent<ICanTakeDamage>();
                        if (target == null)
                            target = hitObject.GetComponentInParent<ICanTakeDamage>();

                        if (target != null)
                        {
                            // Don't damage yourself
                            PlayerCharacter attackingPlayerCharacter = PlayerManager.Get(ownerPlayerRef);
                            ICanTakeDamage attackingPlayer = attackingPlayerCharacter.GetComponent<ICanTakeDamage>();
                            if (attackingPlayer == target)
                                continue;

                            Vector3 targetHeading = (hitObject.transform.position - source).normalized;
                            float angleBetween = Vector3.Angle(ownerPlayer.Visual.transform.forward, targetHeading);

                            if (angleBetween < _collisionSettings.coneAngle * 0.5f)
                            {
                                Vector3 toTarget = Vector3.ProjectOnPlane(hitObject.transform.position - ownerPlayer.transform.position, Vector3.up);
                                Vector3 impulse = (toTarget.normalized + (Vector3.up * 0.1f)) * _collisionSettings.knockBackImpulse;

                                // Remove a tick from the weapon. If max health is zero than ignore.
                                if (weaponElement._baseHealth > 0)
                                    health = Mathf.Clamp(health - 1, 0, weaponElement._baseHealth);

                                target.ApplyDamage(impulse, weaponElement._damage, weaponElement.GetDamageModifiers(), weaponElement.weaponType, ownerPlayerRef);
                            }
                        }
                    }
                }
            }

        }

        public static void OnFireTickChanged(Changed<Weapon> changed)
        {
            changed.Behaviour.FireFx();
        }

        private void FireFx()
        {
            _audioPlayer.PlayOneShot();
        }

        public static void OnHealthChanged(Changed<Weapon> changed)
        {
            if (changed.Behaviour != null)
                changed.Behaviour.HealthChanged();
        }

        private void HealthChanged()
        {
            Debug.LogFormat("<color=red>{0} Health Changed: Health = {1}</color>", weaponElement.weaponType ,health);

            if (Object.HasInputAuthority)
            {
                var weaponHealthUI = FindObjectOfType<WeaponHealthUI>();
                if (weaponHealthUI != null)
                {
                    int index = Mathf.Clamp((weaponElement._baseHealth - 1) - health, 0, weaponElement._baseHealth - 1);
                    weaponHealthUI.ToggleHealthTick(index, false);
                }
            }

            if (Object.HasStateAuthority)
            {
                if (health <= 0)
                {
                    Debug.Log("<color= red>Weapon Destroyed!</color>");
                    _weaponManager.DropActiveWeapon();
                }
            }
        }

#if UNITY_EDITOR
        [ExecuteInEditMode]
        private void OnDrawGizmos()
        {
            PlayerCharacter ownerPlayer;
            if (transform.root.TryGetComponent<PlayerCharacter>(out ownerPlayer))
            {
                Gizmos.color = Color.green;
                Vector3 source = ownerPlayer.transform.position + ownerPlayer.Visual.transform.forward * _collisionSettings.offset.z;
                source += ownerPlayer.Visual.transform.right * _collisionSettings.offset.x;
                source += ownerPlayer.Visual.transform.up * _collisionSettings.offset.y;
                Handles.DrawWireDisc(source, ownerPlayer.Visual.transform.up, _collisionSettings.radius);
                Gizmos.color = Color.red;
                Gizmos.DrawLine(source, source + (Quaternion.AngleAxis(_collisionSettings.coneAngle * 0.5f, Vector3.up) * ownerPlayer.Visual.transform.forward) * _collisionSettings.radius);
                Gizmos.DrawLine(source, source + (Quaternion.AngleAxis(_collisionSettings.coneAngle * -0.5f, Vector3.up) * ownerPlayer.Visual.transform.forward) * _collisionSettings.radius);

            }
        }
#endif
    }
}
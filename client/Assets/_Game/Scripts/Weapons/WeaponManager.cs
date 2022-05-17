using System;
using UnityEngine;
using Fusion;
using BeamableExample.RedlightGreenLight.Character;

namespace BeamableExample.RedlightGreenLight
{
    public class WeaponManager : NetworkBehaviour
    {
        [SerializeField] private Weapon[] _weapons;
        [SerializeField] private PlayerCharacter _player;

        [HideInInspector]
        [Networked]
        public TickTimer PrimaryFireDelay { get; set; }

        public Action<Weapon> WeaponChangedEvent;

        [Networked(OnChanged = nameof(OnActiveWeaponIndexChanged))]
        private byte _activeWeaponIndex { get; set; }

        public override void Spawned()
        {
            ClearWeapons();
        }

        public void ClearWeapons()
        {
            foreach (Weapon weapon in _weapons)
            {
                weapon.SetActive(false);
            }

            _activeWeaponIndex = 0;

            // Activate the punch weapon by default.
            ActivateWeapon(0);
        }

        public void DropActiveWeapon()
        {
            if (Object.HasStateAuthority)
            {
                Debug.Log("<color= red>DropActiveWeapon()</color>");
                _activeWeaponIndex = 0;
                RPC_DisableWeaponEffects();
            }
        }

        [Rpc(sources: RpcSources.All, targets: RpcTargets.All, InvokeLocal = true, Channel = RpcChannel.Reliable)]
        public void RPC_DisableWeaponEffects()
        {
            if (this.TryGetComponent(out CharacterFXManager characterFX))
            {
                Debug.Log("<color= red>RPC_DisableWeaponEffects()</color>");
                characterFX.DisableAllEffects();
            }
        }

        public void PickUpWeapon(WeaponElement weapon)
        {
            int weaponIndex = GetWeaponIndex(weapon.weaponType);

            if (Object.HasInputAuthority == false)
            {
                RPC_SetActiveWeaponIndex(weaponIndex);
            }
            else
            {
                SetActiveWeaponIndex(weaponIndex);
            }
        }

        [Rpc(sources: RpcSources.All, targets: RpcTargets.All, InvokeLocal = true, Channel = RpcChannel.Reliable)]
        public void RPC_SetActiveWeaponIndex(int weaponIndex)
        {
            SetActiveWeaponIndex(weaponIndex);
        }

        public void SetActiveWeaponIndex(int weaponIndex)
        {
            _activeWeaponIndex = (byte)weaponIndex;
        }

        private static void OnActiveWeaponIndexChanged(Changed<WeaponManager> changed)
        {
            
            // Get the changed value.
            int newWeaponIndex = changed.Behaviour._activeWeaponIndex;
            changed.LoadOld();
            var oldWeaponIndex = changed.Behaviour._activeWeaponIndex;

            Debug.LogFormat("<color= red>OnActiveWeaponIndexChanged() - {0}</color>", newWeaponIndex);

            if (changed.Behaviour)
            {
                if (oldWeaponIndex != newWeaponIndex)
                    changed.Behaviour.ActivateWeapon(newWeaponIndex);
            }
        }

        /// <summary>
        /// Activate a new weapon when picked up
        /// </summary>
        /// <param name="weaponIndex">Index of weapon the _Weapons list for the player</param>
        public void ActivateWeapon(int weaponIndex)
        {
            if (!_player.IsActivated)
                return;

            Debug.LogFormat("WeaponManager - ActivateWeapon - Player_{0}", Object.InputAuthority);

            // Fail safe, clamp the weapon index within weapons list bounds
            weaponIndex = Mathf.Clamp(weaponIndex, 0, _weapons.Length - 1);

            for (int i = 0; i < _weapons.Length; i++)
            {
                if (i == (byte)weaponIndex)
                {
                    _weapons[i].SetActive(true);

                    PlayerCharacter targetPlayer = PlayerManager.Get(Object.InputAuthority);
                    targetPlayer.SetWeaponLayer(_weapons[(byte)weaponIndex].weaponElement.AnimationLayer);
                    WeaponChangedEvent?.Invoke(_weapons[(byte)weaponIndex]);
                    PlayerInputListener.Instance.Action = false;

                    if (Object.HasInputAuthority)
                    {
                        var weaponHealthUI = FindObjectOfType<WeaponHealthUI>();
                        if (weaponHealthUI != null)
                        {
                            weaponHealthUI.AddHealthTicks(_weapons[i].weaponElement._baseHealth);
                        }
                    }
                }
                else
                {
                    _weapons[i].SetActive(false);
                }
            }
        }

        /// <summary>
        /// Fire the current weapon. This is called from the Input Auth Client and on the Server in
        /// response to player input.
        /// </summary>
        public void FireWeapon()
        {
            if (!IsWeaponFireAllowed())
                return;

            TickTimer tickTimer = PrimaryFireDelay;
            if (tickTimer.ExpiredOrNotRunning(Runner))
            {

                byte weaponIndex = _activeWeaponIndex;
                Weapon weapon = _weapons[weaponIndex];

                weapon.Fire(Runner, Object.InputAuthority);

                PrimaryFireDelay = TickTimer.CreateFromSeconds(Runner, weapon.weaponElement._rateOfFire);
            }
        }

        private bool IsWeaponFireAllowed()
        {
            if (!_player.IsActivated)
                return false;

            return true;
        }

        private int GetWeaponIndex(WeaponType weaponType)
        {
            int index = Array.FindIndex(_weapons, 0, _weapons.Length, w => w.weaponElement.weaponType == weaponType);
            if (index == -1)
            {
                Debug.LogError($"Weapon {weaponType} was not found in the weapon list, returning <color=red>0 </color>");
                return 0;
            }
            return index;
        }
    }
}
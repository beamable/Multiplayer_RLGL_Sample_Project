using Fusion;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BeamableExample.RedlightGreenLight
{
	/// <summary>
	/// Powerups are spawned by the LevelManager and, when picked up, changes the
	/// current weapon of the tank.
	/// </summary>
	public class WeaponSpawner : NetworkBehaviour, ISpawnableNetworkLevelObject
	{
		[SerializeField] private WeaponElement[] _weaponElements;
		[SerializeField] private Renderer _renderer;
		[SerializeField] private MeshFilter _meshFilter;

		[Networked(OnChanged = nameof(OnRespawningChanged))]
		public NetworkBool IsRespawning { get; set; }

		[Networked(OnChanged = nameof(OnActiveWeaponIndexChanged))]
		public int ActiveWeaponIndex { get; set; }

		[Networked]
		public float RespawnTimerFloat { get; set; }

		private float _respawnDuration = 3f;
		public float RespawnProgress => RespawnTimerFloat / _respawnDuration;

		private PickupManager _pickupManager;
		
		public void Initialize()
		{
			SetNextWeapon();
		}

		public override void Spawned()
		{
			_renderer.enabled = false;
			IsRespawning = true;

			_pickupManager = FindObjectOfType<PickupManager>(true);
			_pickupManager.AddPickupIndicator(this);
		}

		public override void FixedUpdateNetwork()
		{
			if (!Object.HasStateAuthority)
				return;

			// Update the respawn timer
			RespawnTimerFloat = Mathf.Min(RespawnTimerFloat + Runner.DeltaTime, _respawnDuration);

			// Spawn a new powerup whenever the respawn duration has been reached
			if (RespawnTimerFloat >= _respawnDuration && IsRespawning)
			{
				IsRespawning = false;
			}
		}

		// Create a simple scale in effect when spawning
		public override void Render()
		{
			if (!IsRespawning)
			{
				_renderer.transform.localScale = Vector3.Lerp(_renderer.transform.localScale, Vector3.one, Time.deltaTime * 5f);
			}
			else
			{
				_renderer.transform.localScale = Vector3.zero;
			}
		}

		/// <summary>
		/// Get the pickup contained in this spawner and trigger the spawning of a new weapon.
		/// </summary>
		/// <returns></returns>
		public WeaponElement Pickup()
		{
			if (IsRespawning)
				return null;

			// Store the active weapon index for returning
			int lastIndex = ActiveWeaponIndex;

			// Trigger the pickup effect, hide the weapon and select the next weapon to spawn
			if (RespawnTimerFloat >= _respawnDuration)
			{
				if (_renderer.enabled)
				{
					AudioPlayer audioPlayer = GetComponent<AudioPlayer>();
					if (audioPlayer != null)
						audioPlayer.PlayOneShot(_weaponElements[lastIndex].pickupSnd);
					_renderer.enabled = false;
					SetNextWeapon();
				}
			}
			return lastIndex != -1 ? _weaponElements[lastIndex] : null;
		}

		private void SetNextWeapon()
		{
			if (Object.HasStateAuthority)
			{
				ActiveWeaponIndex = Random.Range(0, _weaponElements.Length);
				RespawnTimerFloat = 0;
				IsRespawning = true;
			}
		}

		public static void OnActiveWeaponIndexChanged(Changed<WeaponSpawner> changed)
		{
		}

		public static void OnRespawningChanged(Changed<WeaponSpawner> changed)
		{
			if(changed.Behaviour)
				changed.Behaviour.OnRespawningChanged();
		}

		private void OnRespawningChanged()
		{
			_renderer.enabled = true;
			_meshFilter.mesh = _weaponElements[ActiveWeaponIndex].spawnerMesh;

            // TODO:: we need to change the pickup image.
            _pickupManager.SetWeaponPickupSprite(this, _weaponElements[ActiveWeaponIndex].pickupSprite);
        }
	}
}
using UnityEngine;

namespace BeamableExample.RedlightGreenLight
{
	[System.Flags]
	public enum DamageModifiers
	{
		NONE = (1 << 0),
		KNOCKBACK = (1 << 1),
		FIRE = (1 << 2),
		ICE = (1 << 3)
	}

	public enum WeaponType
    {
		NONE,
		BAT,
		KNIFE,
		MELEE,
		LASER
    }

	[CreateAssetMenu(fileName = "WPN_", menuName = "ScriptableObjects/WeaponType")]
	public class WeaponElement : ScriptableObject
	{
		public WeaponType weaponType;
		[SerializeField] [EnumFlags] DamageModifiers _damageMods;
		public int AnimationLayer;
		public float _rateOfFire;
		public int _damage;
		public int _baseHealth;
		public Mesh spawnerMesh;
		public AudioClipData impactSnd;
		public AudioClipData pickupSnd;
		public Sprite pickupSprite;

		public DamageModifiers GetDamageModifiers()
        {
			return _damageMods;
        }
	}
}
using Fusion;
using UnityEngine;

namespace BeamableExample.RedlightGreenLight
{
	/// <summary>
	/// Interface implemented by any gameobject that can be damaged.
	/// </summary>
	public interface ICanTakeDamage
	{
		void ApplyDamage(Vector3 impulse, int damage, DamageModifiers damageType, WeaponType weaponType, PlayerRef source);
	}
}
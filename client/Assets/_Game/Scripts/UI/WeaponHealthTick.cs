using UnityEngine;
using UnityEngine.UI;

namespace BeamableExample.RedlightGreenLight
{
	public class WeaponHealthTick : MonoBehaviour
	{
		[SerializeField] private Toggle _tickToggle;

		public void Initialize()
		{
			Toggle(true);
		}

        public void Toggle(bool value)
        {
			_tickToggle.isOn = value;
		}
	}
}
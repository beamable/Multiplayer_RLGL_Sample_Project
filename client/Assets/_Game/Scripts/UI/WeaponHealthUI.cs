using System.Collections.Generic;
using UnityEngine;

namespace BeamableExample.RedlightGreenLight
{
    public class WeaponHealthUI : MonoBehaviour
    {
        [SerializeField] private Transform _weaponHealthTickParent;
        [SerializeField] private WeaponHealthTick _weaponHealthTickPrefab;

        private List<WeaponHealthTick> _healthTicks = new List<WeaponHealthTick>();

        public void AddHealthTicks(int ticks)
        {
            ClearHealthTicksPickupIndicator();

            for (int i = 0; i < ticks; i++)
            {
                var tick = Instantiate(_weaponHealthTickPrefab, _weaponHealthTickParent);
                _healthTicks.Add(tick);

                tick.Initialize();
            }
        }

        public void ClearHealthTicksPickupIndicator()
        {
            foreach (WeaponHealthTick tick in _healthTicks)
            {
                Destroy(tick.gameObject);
            }

            _healthTicks.Clear();
        }

        public void ToggleHealthTick(int index, bool value)
        {
            if (_healthTicks == null)
                return;

            if (index < _healthTicks.Count && index >= 0)
                _healthTicks[index].Toggle(value);
        }
    }
}

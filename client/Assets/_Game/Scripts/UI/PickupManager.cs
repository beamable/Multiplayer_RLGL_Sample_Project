using System.Collections.Generic;
using UnityEngine;

namespace BeamableExample.RedlightGreenLight
{
    public class PickupManager : MonoBehaviour
    {
        [SerializeField] private Transform _pickupUIParent;
        [SerializeField] private PickupIndicator _pickupPrefab;
        [SerializeField] private PickupIndicator pickupUI;

        private Dictionary<WeaponSpawner, PickupIndicator> _pickupUIs = new Dictionary<WeaponSpawner, PickupIndicator>();

        public void AddPickupIndicator(WeaponSpawner weaponSpawner)
        {
            PickupIndicator indicator;
            if (!_pickupUIs.TryGetValue(weaponSpawner, out indicator))
            {
#if !UNITY_ANDROID && !UNITY_IOS
                indicator = Instantiate(_pickupPrefab, _pickupUIParent);
#else
                indicator = pickupUI;
#endif
                _pickupUIs.Add(weaponSpawner, indicator);

                indicator.Initialize(weaponSpawner);
            }
        }

        public void RemovePickupIndicator(WeaponSpawner weaponSpawner)
        {
            _pickupUIs.Remove(weaponSpawner);
        }

        public void ShowPickupIndicator(WeaponSpawner weaponSpawner)
        {
            PickupIndicator indicator;
            if (_pickupUIs.TryGetValue(weaponSpawner, out indicator))
            {
                indicator.Show(true);
            }
        }

        public void HidePickupIndicator(WeaponSpawner weaponSpawner)
        {
            PickupIndicator indicator;
            if (_pickupUIs.TryGetValue(weaponSpawner, out indicator))
            {
                indicator.Show(false);
                indicator.UpdateRing(0.0f);
            }
        }

        public void HidePickupIndicators()
        {
            foreach(KeyValuePair<WeaponSpawner, PickupIndicator> pair in _pickupUIs)
            {
                pair.Value.Show(false);
                pair.Value.UpdateRing(0.0f);
            }
        }

        public void UpdateIndicatorRing(WeaponSpawner weaponSpawner, float value)
        {
            PickupIndicator indicator;
            if (_pickupUIs.TryGetValue(weaponSpawner, out indicator))
            {
                indicator.UpdateRing(value);
            }
        }

        public void SetWeaponPickupSprite(WeaponSpawner weaponSpawner, Sprite weaponSprite)
        {
            PickupIndicator indicator;
            if (_pickupUIs.TryGetValue(weaponSpawner, out indicator))
            {
                indicator.SetWeaponImage(weaponSprite);
            }
        }
    }
}

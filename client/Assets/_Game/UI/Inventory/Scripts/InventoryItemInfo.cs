using Beamable.Common.Api.Inventory;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace _Game.UI.Inventory.Scripts
{
    public class InventoryItemInfo : MonoBehaviour
    {
        public UnityEvent<string, string> OnItemEquipped;
        
        [SerializeField]
        private TextMeshProUGUI itemTitle;
        [SerializeField]
        private TextMeshProUGUI itemDescription;

        private ItemView _selectedItem;

        [UsedImplicitly]
        public void SetInfo(ItemView item)
        {
            _selectedItem = item;

            itemTitle.text = item.properties["title"];
            itemDescription.text = item.properties["description"];
        }

        [UsedImplicitly]
        public void EquipItem()
        {
            if (_selectedItem == null)
            {
                return;
            }
            OnItemEquipped?.Invoke(_selectedItem.properties["type"], _selectedItem.properties["title"]);
        }

        [UsedImplicitly]
        public void ClearInfo()
        {
            itemTitle.text = "";
            itemDescription.text = "";
        }
    }
}
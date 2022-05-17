using System.Collections.Generic;
using Beamable.Api.Payments;
using Beamable.Common.Api.Inventory;
using Beamable.Common.Inventory;
using UnityEngine;
using UnityEngine.Events;

namespace _Game.Features.Customization.Scripts
{
    /// <summary>
    /// A helper class to display a preview of a customization item on the character.
    /// This class exists to minimize the Store and CustomizationController's dependency on each other.
    /// </summary>
    public class PlayerCustomizationPreview : MonoBehaviour
    {
        [SerializeField]
        private CharacterCustomizationLobby _characterCustomization;
        
        private void Start()
        {
            if (_characterCustomization == null)
            {
                _characterCustomization = GetComponent<CharacterCustomizationLobby>();
            }
            _characterCustomization.Initialize();
        }
        
        public UnityEvent<string, string> OnPreviewUpdated;
        
        /// <summary>
        /// Updates the customization display with the selected offer.
        /// </summary>
        public void PreviewChange(PlayerOfferView offer)
        {
            if (offer.obtainItems == null || offer.obtainItems.Count == 0) return;
            
            var obtainItem = offer.obtainItems[0];
            var category = GetObtainItemProperty(obtainItem, "type");
            var title = GetObtainItemProperty(obtainItem, "title");
            OnPreviewUpdated?.Invoke(category, title);
        }
        
        public void PreviewChange(ItemView item)
        {
            var category = item.properties["type"];
            var title = item.properties["title"];
            OnPreviewUpdated?.Invoke(category, title);
        }

        private string GetObtainItemProperty(ObtainItem obtainItem, string propertyName)
        {
            var property = obtainItem.properties.Find(item => item.name == propertyName);
            return property == null ? string.Empty : property.value;
        }
    }
}

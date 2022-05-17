using System;
using System.Threading.Tasks;
using Beamable.Common.Inventory;
using Beamable.UI.Scripts;
using ListView;
using UnityEngine.UI;

namespace _Game.UI.Inventory.Scripts
{
    public class InventoryItemCard : ListCard
    {
        public Image Icon;
        public override async void SetUp(ListItem item)
        {
            try
            {
                var itemContent = await GetObtainItemContent(item.Title);
                if (Icon == null || itemContent == null) return;
                Icon.sprite = await itemContent.icon.LoadSprite();
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log(e.Message);
            }
        }
        private async Task<ItemContent> GetObtainItemContent(string contentId)
        {
            return await new ItemRef { Id = contentId }.Resolve();
        }
    }
}
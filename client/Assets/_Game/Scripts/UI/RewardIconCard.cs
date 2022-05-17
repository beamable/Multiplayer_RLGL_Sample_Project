using System.Threading.Tasks;
using Beamable.Common.Inventory;
using Beamable.Microservices;
using Beamable.UI.Scripts;
using ListView;
using UnityEngine;
using UnityEngine.UI;

public class RewardIconCard : ListCard
{
    public Image icon;
    public override async void SetUp(ListItem item)
    {
        var reward = item.PropertyBag["reward"] as Reward;
        if (icon == null) return;
        
        if (reward == null) return;
        if (reward.IsItem)
        {
            var itemContent = await GetObtainItemContent(reward.ItemContentId);
            if (itemContent != null && itemContent.icon != null)
            {
                icon.sprite = await itemContent.icon.LoadSprite();
            }
            else
            {
                icon.gameObject.SetActive(false);
            }
        }
        else
        {
            var currencyContent = await GetObtainCurrencyContent(reward.CurrencyType);
            if (currencyContent != null && currencyContent.icon != null)
            {
                icon.sprite = await currencyContent.icon.LoadSprite();
            }
            else
            {
                icon.gameObject.SetActive(false);
            }
        }
    }

    private async Task<ItemContent> GetObtainItemContent(string contentId)
    {
        return await new ItemRef { Id = contentId }.Resolve();
    }
    
    private async Task<CurrencyContent> GetObtainCurrencyContent(string symbol)
    {
        return await new CurrencyRef { Id = symbol }.Resolve();
    }

}

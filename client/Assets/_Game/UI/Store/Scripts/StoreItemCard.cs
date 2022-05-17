using Beamable.Common.Inventory;
using Beamable.UI.Scripts;
using ListView;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StoreItemCard : ListCard
{
    [SerializeField] private TextMeshProUGUI price;
    [SerializeField] private Image image;
    [SerializeField] private Image amountBackground;
    [SerializeField] private Sprite ownedBackground;

    public override void SetUp(ListItem item)
    {
        if (item.PropertyBag.ContainsKey("item_content") && item.PropertyBag["item_content"] != null)
        {
            var currentPrice = double.Parse(item.PropertyBag["price"].ToString());
            price.text = $"{currentPrice:n0}";
            SetImage(item.PropertyBag["item_content"] as ItemContent);
            if ((bool) item.PropertyBag["has_item"])
            {
                amountBackground.sprite = ownedBackground;
                price.text = "OWNED";
            }
        }

        if (item.PropertyBag.ContainsKey("currency_content") && item.PropertyBag["currency_content"] != null)
        {
            var currentPrice = double.Parse(item.PropertyBag["price"].ToString()) / 100;
            price.text = $"${currentPrice:n2}";
            SetImage(null, item.PropertyBag["currency_icon"] as Sprite);
        }
    }

    private async void SetImage(ItemContent itemContent, Sprite currencyIcon = null)
    {
        if (itemContent != null)
        {
            image.sprite = await itemContent.icon.LoadSprite();
        }

        if (currencyIcon != null)
        {
            image.sprite = currencyIcon;
        }
    }
}

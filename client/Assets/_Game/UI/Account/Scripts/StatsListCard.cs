using ListView;
using TMPro;
using UnityEngine;

namespace _Game.UI.Account.Scripts
{
    public class StatsListCard : ListCard
    {
        [SerializeField] private TextMeshProUGUI numberLabel;

        public override void SetUp(ListItem item)
        {
            SetTitle(item.Title);
            
            if (item.PropertyBag.ContainsKey("amount"))
            {
                numberLabel.text = item.PropertyBag["amount"] as string;
            }
        }
    }
}

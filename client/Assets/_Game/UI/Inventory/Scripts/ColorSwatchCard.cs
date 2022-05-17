using System.Collections;
using System.Collections.Generic;
using ListView;
using UnityEngine;
using UnityEngine.UI;

public class ColorSwatchCard : ListCard
{
    [SerializeField] private Image swatch;
    [SerializeField] private GameObject highlight;

    public override void SetUp(ListItem item)
    {
        if (item.PropertyBag.ContainsKey("color"))
        {
            if(ColorUtility.TryParseHtmlString(item.PropertyBag["color"] as string, out var color))
            {
                swatch.color = color;
            }
        }
    }
}

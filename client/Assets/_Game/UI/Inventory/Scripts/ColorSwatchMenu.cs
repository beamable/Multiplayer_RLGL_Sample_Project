using System.Collections;
using System.Collections.Generic;
using Beamable.Stats;
using JetBrains.Annotations;
using ListView;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ColorSwatchMenu : MonoBehaviour
{
    [SerializeField] private GridLayoutGroup gridLayout;
    [SerializeField] private int maxColumns = 5;
    [SerializeField] private ListViewComponent listViewComponent;
    [SerializeField] private UnityEvent<string, string> OnSelectionChanged;
    [SerializeField] private StatObject hairColorStat, bodyColorStat;
    [SerializeField] private ColorPalette palette;
    private StatObject _statToChange;

    private void Start()
    {
        SetToColorHair();
    }

    [UsedImplicitly]
    public void UpdateList()
    {
        if (maxColumns > palette.colors.Count)
        {
            maxColumns = palette.colors.Count;
        }
        gridLayout.constraintCount = maxColumns;

        var cardData = new ListViewData();

        foreach (var color in palette.colors)
        {
            cardData.Add(new ListItem
            {
                PropertyBag = new Dictionary<string, object>()
                {
                    {"color", "#" + ColorUtility.ToHtmlStringRGB(color)}
                },
                ListPrefabIndex = 0,
                ViewAction = () =>
                {
                    OnSelectionChanged?.Invoke(_statToChange.StatKey, ColorUtility.ToHtmlStringRGB(color));
                }
            });
        }
        listViewComponent.Build(cardData);
    }

    [UsedImplicitly]
    public void SetToColorHair()
    {
        _statToChange = hairColorStat;
    }

    [UsedImplicitly]
    public void SetToColorBody()
    {
        _statToChange = bodyColorStat;
    }
}

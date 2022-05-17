using System;
using System.Collections.Generic;
using System.Linq;
using Beamable.Api.Payments;
using UnityEngine;
using Beamable.Common.Api.Inventory;
using Beamable.Common.Inventory;
using JetBrains.Annotations;
using ListView;
using UnityEngine.Events;
using UnityEngine.UI;

public class InventoryMenu : MonoBehaviour
{
    [SerializeField]
    private ListViewComponent listViewComponent;

    [SerializeField] private GameObject noItemsUI;
    [SerializeField] private GameObject itemDetailsUI;
    [SerializeField] private GameObject colorSwatchUI;
    [SerializeField] private GameObject scrollbar;
    [SerializeField] private ScrollRect scrollRect;

    [SerializeField]
    private UnityEvent<ItemView> OnSelectionChanged;

    private InventoryView _inventory;
    private bool _hasInventory;

    [UsedImplicitly]
    public void UpdateList(InventoryView inventory)
    {
        var cardData = new ListViewData();
        _inventory = inventory;

        foreach (var itemKvp in inventory.items)
        {
            cardData.AddRange(itemKvp.Value.Select(item =>
            {
                var properties = item.properties.ToDictionary<KeyValuePair<string, string>, string, object>(
                    kvp => kvp.Key,
                    kvp => kvp.Value);
                return new ListItem
                {
                    Id = item.id.ToString(),
                    Title = itemKvp.Key,
                    PropertyBag = properties,
                    ListPrefabIndex = 0,
                    ViewAction = () => { OnSelectionChanged?.Invoke(item); }
                };
            }));
        }

        listViewComponent.Build(cardData);

        _hasInventory = cardData.Count > 0;
        SetInventoryUI();
    }

    [UsedImplicitly]
    public void SelectInitialItem()
    {
        try
        {
            if (_inventory.items.Count == 0)
            {
                return;
            }

            OnSelectionChanged?.Invoke(_inventory.items.First().Value.First());
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    public void SetInventoryUI()
    {
        noItemsUI.SetActive(!_hasInventory);
        itemDetailsUI.SetActive(_hasInventory);
        colorSwatchUI.SetActive(_hasInventory);
        scrollbar.SetActive(_hasInventory);
        scrollRect.enabled = _hasInventory;
    }

    [UsedImplicitly]
    public void HideInventoryList(bool showInventory)
    {
        listViewComponent.gameObject.SetActive(!showInventory);
    }
}
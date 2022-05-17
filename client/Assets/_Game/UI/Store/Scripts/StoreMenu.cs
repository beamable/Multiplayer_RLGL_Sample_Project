using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _Game.CustomContent;
using Beamable;
using Beamable.Api.Payments;
using Beamable.Common.Api.Inventory;
using Beamable.Common.Inventory;
using Beamable.Common.Shop;
using Beamable.UI.Scripts;
using JetBrains.Annotations;
using ListView;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class StoreMenu : MonoBehaviour
{
    public UnityEvent<PlayerOfferView, ListingContent> OnAttemptPurchase;
    public UnityEvent<PlayerOfferView> OnSelectionChanged;

    [SerializeField]
    private ListViewComponent listViewComponent;

    [SerializeField] private TextMeshProUGUI itemTitle;
    [SerializeField] private TextMeshProUGUI itemDescription;
    [SerializeField] private GameObject buyButton;
    [SerializeField] private GameObject ownedButton;
    [SerializeField] private TextMeshProUGUI confirmText;

    private PlayerStoreView _store;

    private PlayerOfferView _selectedItem;
    private ListingContent _selectedListing;
    
    private IBeamableAPI _beamableAPI;
    private InventoryView _currentInventory;

    private async void OnEnable()
    {
        await SetUpBeamable();
        _currentInventory = await _beamableAPI.InventoryService.GetCurrent();
    }

    private async Task SetUpBeamable()
    {
        _beamableAPI = await API.Instance;
    }

    private PlayerOfferView SelectedItem
    {
        get => _selectedItem;
        set
        {
            _selectedItem = value;
            OnSelectionChanged?.Invoke(_selectedItem);
        }
    }

    [UsedImplicitly]
    public async void UpdateList(PlayerStoreView store)
    {
        var cardData = new ListViewData();
        _store = store;

        if (_store?.listings == null) return;
        foreach (var listing in store.listings)
        {
            var offer = listing.offer;
            var listingContent = await new ListingRef {Id = listing.symbol}.Resolve();
            CurrencyListingContent currencyListingContent = null;
            var hasItem = false;
            Sprite currencyIcon = null;

            CurrencyContent obtainCurrencyContent = null;
            if (offer.obtainCurrency != null && offer.obtainCurrency.Count > 0)
            {
                obtainCurrencyContent = await GetObtainCurrencyContent(offer.obtainCurrency[0]);
                currencyListingContent = await new CurrencyListingRef {Id = listing.symbol}.Resolve();
                currencyIcon = await currencyListingContent.icon.LoadSprite();

            }

            ItemContent obtainItemContent = null;
            if (offer.obtainItems != null && offer.obtainItems.Count > 0)
            {
                obtainItemContent = await GetObtainItemContent(offer.obtainItems[0]);
                foreach (var item in _currentInventory.items.Where(item => item.Key == obtainItemContent.Id))
                {
                    hasItem = true;
                }
            }
            cardData.Add(new ListItem
            {
                PropertyBag = new Dictionary<string, object>()
                {
                    {"offer", offer},
                    {"price", offer.price.amount},
                    {"item_content", obtainItemContent},
                    {"currency_content", obtainCurrencyContent},
                    {"currency_icon", currencyIcon},
                    {"has_item", hasItem}
                },
                ListPrefabIndex = 0,
                ViewAction = () => { SelectOffer(offer, listingContent, hasItem); }
            });
        }
        
        listViewComponent.Build(cardData);
    }
    
    private async Task<ItemContent> GetObtainItemContent(ObtainItem item)
    {
        return await new ItemRef { Id = item.contentId }.Resolve();
    }
    
    private async Task<CurrencyContent> GetObtainCurrencyContent(ObtainCurrency item)
    {
        return await new CurrencyRef { Id = item.symbol }.Resolve();
    }

    private void SelectOffer(PlayerOfferView offer, ListingContent content, bool owned)
    {
        itemTitle.text = offer.titles[0];
        itemDescription.text = offer.descriptions[0];
        _selectedListing = content;
        SelectedItem = offer;
        buyButton.SetActive(!owned);
        ownedButton.SetActive(owned);
    }

    public void AttemptPurchase()
    {
        OnAttemptPurchase?.Invoke(_selectedItem, _selectedListing);
    }
    
    [UsedImplicitly]
    public async void SelectInitialItem()
    {
        try
        {
            if (_store.listings.Count == 0)
            {
                return;
            }

            OnSelectionChanged?.Invoke(_store.listings.First().offer);
            var listingRef = await new ListingRef {Id = _store.listings.First().symbol}.Resolve();
            var hasItem = false;
            if (listingRef.offer.obtainCurrency != null && listingRef.offer.obtainCurrency.Count > 0)
            {
                hasItem = false;
            }
            else
            {
                hasItem = _currentInventory.items.ContainsKey(listingRef.offer.obtainItems[0].contentId);
            }
            SelectOffer(_store.listings.First().offer, listingRef, hasItem);
        }
        catch(Exception e)
        {
            Debug.LogError(e);
        }
    }

    public void SetConfirmText()
    {
        confirmText.text = _selectedItem.price.type != "sku"
            ? $"Are you sure you want to buy \"{_selectedItem.titles[0]}\" for {_selectedItem.price.amount:n0} Photons?"
            : $"Are you sure you want to buy \"{_selectedItem.titles[0]}\" for {_selectedItem.price.amount * 0.01f:c2}?";
    }

    [UsedImplicitly]
    public void GetCurrentInventory(InventoryView inventory)
    {
        _currentInventory = inventory;
        UpdateList(_store);
    }
}

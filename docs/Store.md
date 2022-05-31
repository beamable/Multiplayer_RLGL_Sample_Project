# Store

### In-Game Usage

#### Categories

The Store page includes category tabs at the top of the item menu to switch the store content. Clicking each category's tab should reload the grid of items with that category. In the sample project, there is a headwear store, body store, and the currency store. The headwear and body store contain accessories for the character, while the currency store contains "Gems", a premium currency available to buy with real money.

#### Player Interaction

Selecting an equippable item will display the item on the character model as a preview within the menu. Under the character preview area, there is a container that will display the selected item's name and description and the buy button. If an item is already in the player's inventory, the item's icon will display with an alternate version of the price tag; the tag will be disabled, grey, and the word "owned" will replace the price text.

The player's currency balance is displayed in additional containers next to the store tabs. Popups will appear when the player attempts to buy an item to confirm their purchase and then to notify them whether the purchase was successful or if it failed and for what reason.

### Class Breakdown

**StoreMenu** - Sets up the UI of the store.

- Uses Beamable's `PlayerStoreView` to get the listings from the selected store, as well as Beamable's listing content to get the listings in the store, and Beamable's `PlayerOfferView` to get the offers in the listings. Here, we are getting some basic data about the store being rendered before further filtering.
- Beamable's inventory service checks what items the player already owns to compare against the listings so that any listed items cannot be purchased again. This applies to equipable items only; currencies may be purchased multiple times.
```csharp
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
```
Next, we need to check if the item primarily contains currency, such as the items in the Gems store, where the player is using real money to buy virtual currency. Since we are only utilizing one obtain currency or obtain item per listing, we only need to check the first index of `offer.obtainCurrency`. In more complex use cases, more advanced filtering may be required to render it properly.
```csharp
        if (offer.obtainCurrency != null && offer.obtainCurrency.Count > 0)
        {
            obtainCurrencyContent = await GetObtainCurrencyContent(offer.obtainCurrency[0]);
            currencyListingContent = await new CurrencyListingRef {Id = listing.symbol}.Resolve();
            currencyIcon = await currencyListingContent.icon.LoadSprite();
        }
```
Next, we will perform a similar operation with the item content,
```csharp
        ItemContent obtainItemContent = null;
        if (offer.obtainItems != null && offer.obtainItems.Count > 0)
        {
            obtainItemContent = await GetObtainItemContent(offer.obtainItems[0]);
            //Inside this conditional, we're also checking if the player already owns the item.
            //If so, the purchase label will be grayed out, as described earlier.
            foreach (var item in _currentInventory.items.Where(item => item.Key == obtainItemContent.Id))
            {
                hasItem = true;
            }
        }
```
Finally, we can follow through with the standard ListView building procedure. For more info on this pattern, check out the [ListViewComponent documentation](./ListViewComponent.md).
```csharp
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
```

**StoreController** - Gets and sets the store from content, handles purchases, and subscribes to changes in the store and in the player's currency inventory.

- Beamable's CommerceService is used to subscribe to any store or currency changes.
```csharp
private void SubscribeToChanges()
{
    _context.Api.CommerceService.Subscribe(_storeContent.Id, CommerceService_OnChanged);
    _context.Api.InventoryService.Subscribe(_currencyContent.Id, Currency_OnChanged);
}

private void Currency_OnChanged(InventoryView inventoryViewCurrency)
{
    foreach (var kvp in inventoryViewCurrency.currencies)
    {
        onCurrencyUpdatedModels
            .Find(currency => kvp.Key == currency.id)
            .onChangedEvent?.Invoke((int)kvp.Value);
    }
}

//onStoreUpdated is subscribed to by StoreMenu, this triggers a redraw of the Store list
private void CommerceService_OnChanged(PlayerStoreView storeView)
{
    onStoreUpdated?.Invoke(storeView);
}
```
- The CommerceService is also used to make purchases.
```csharp
public async void Buy(PlayerOfferView item, ListingContent listing = null)
{
    if (!(await CanAffordItem(item)))
    {
        Debug.LogError("Cannot afford selected item to purchase");
        onPurchaseFailed?.Invoke();
        return;
    }

    //Check for real-money transactions
    if (listing != null && listing.price.type == "sku")
    {
        var transaction = new PurchaseTransaction(_storeContent.Id, listing.Id, BeamContext.Default);
        transaction.OnPurchaseFailed.AddListener((error) =>
        {
            //Triggers a failure popup message.
            onPurchaseFailed?.Invoke();
        });
        transaction.OnPurchaseSuccess.AddListener((tx) =>
        {
            //Triggers a success popup message.
            onPurchase?.Invoke();
        });
    }
    else
    {
        await _context.Api.CommerceService.Purchase(_storeContent.Id, item.symbol).Error((error) =>
        {
            Debug.LogError(error);
            onPurchaseFailed?.Invoke();
        });
        //The TOTAL_SPENT stat is used for an achievement. This is also a useful place to log metrics if you want to track player purchasing patterns.
        if (listing.price.symbol == "currency.photons")
        {
            await BeamableStatsController.AddToStat(TOTAL_SPENT_KEY, item.price.amount);
        }
        onPurchase?.Invoke();
    }
}
```

> To learn more about the Store feature, read more on Beamable's documentation site [here](https://docs.beamable.com/docs/stores-feature-overview).

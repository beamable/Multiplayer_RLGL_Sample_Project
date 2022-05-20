using System.Collections.Generic;
using System.Threading.Tasks;
using _Game.UI.Store.Scripts;
using Beamable;
using Beamable.Api.Payments;
using Beamable.Common.Api.Inventory;
using Beamable.Common.Inventory;
using Beamable.Common.Shop;
using Beamable.IAP;
using Beamable.Server.Clients;
using JetBrains.Annotations;
using ListView;
using UnityEngine;
using UnityEngine.Events;

public class StoreController : MonoBehaviour
{
    public UnityEvent<PlayerStoreView> onStoreUpdated;
    public List<CurrencyUpdatedModel> onCurrencyUpdatedModels;
    public UnityEvent onPurchaseFailed;
    public UnityEvent onPurchase;

    [SerializeField] private List<StoreRef> stores;
    [SerializeField] private StoreRef storeRef;
    [SerializeField] private CurrencyRef currencyRef;

    private StoreContent _storeContent;
    private CurrencyContent _currencyContent;
    private BeamContext _context;

    private const string TOTAL_SPENT_KEY = "TOTAL_SPENT";

    /// <summary>
    /// Sets up Beamable, gets the current inventory, then subscribes to changes.
    /// <remarks>See <see cref="SetUpBeamable"/>, <see cref="ResolveRefs"/> and <see cref="SubscribeToChanges"/>.</remarks>
    /// </summary>
    [UsedImplicitly]
    public async void SetUp()
    {
        await SetUpBeamable();
        await ResolveRefs();
        SubscribeToChanges();
    }
    
    /// <summary>
    /// Initializes the Beamable API.
    /// </summary>
    private async Task SetUpBeamable()
    {
        _context = BeamContext.Default;
        await _context.OnReady;
    }

    /// <summary>
    /// Sets which store should be referenced.
    /// <remarks>See <see cref="ResolveRefs"/>.</remarks>
    /// </summary>
    /// <param name="storesIndex"></param>
    public async void SetStoreRef(int storesIndex)
    {
        storeRef = stores[storesIndex];
        await ResolveRefs();
        SubscribeToChanges();
    }

    /// <summary>
    /// Returns a bool based on if the user has enough currency for the item.
    /// </summary>
    /// <param name="item"></param>
    private async Task<bool> CanAffordItem(PlayerOfferView item)
    {
        var currency = await _context.Api.InventoryService.GetCurrency(currencyRef.Id);
        return (int)currency >= item.price.amount;
    }

    /// <summary>
    /// Attempts purchase the item and invokes the <see cref="onPurchaseFailed"/> callback if that fails.
    /// </summary>
    /// <param name="item"></param>
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
                onPurchaseFailed?.Invoke(); //TODO: string popup message.
            });
            transaction.OnPurchaseSuccess.AddListener((tx) =>
            {
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
            if (listing.price.symbol == "currency.photons")
            {
                await BeamableStatsController.AddToStat(TOTAL_SPENT_KEY, item.price.amount);
            }
            onPurchase?.Invoke();
        }
    }

    /// <summary>
    /// Invokes the <see cref="onStoreUpdated"/> callback.
    /// </summary>
    /// <param name="storeView"></param>
    private void CommerceService_OnChanged(PlayerStoreView storeView)
    {
        onStoreUpdated?.Invoke(storeView);
    }

    /// <summary>
    /// Sets the currency cached on this component, and invokes the <see cref="onCurrencyUpdatedModels"/> callback.
    /// </summary>
    /// <param name="inventoryViewCurrency"></param>
    private void Currency_OnChanged(InventoryView inventoryViewCurrency)
    {
        foreach (var kvp in inventoryViewCurrency.currencies)
        {
            onCurrencyUpdatedModels
                .Find(currency => kvp.Key == currency.id)
                .onChangedEvent?.Invoke((int)kvp.Value);
        }
    }
    

    /// <summary>
    /// Resolves the storeRef and currencyRef to get their content.
    /// </summary>
    private async Task ResolveRefs()
    {
        _storeContent = await storeRef.Resolve();
        _currencyContent = await currencyRef.Resolve();
    }

    /// <summary>
    /// Subscribes to changes to the store and the user's currency.
    /// </summary>
    private void SubscribeToChanges()
    {
        _context.Api.CommerceService.Subscribe(_storeContent.Id, CommerceService_OnChanged);
        _context.Api.InventoryService.Subscribe(_currencyContent.Id, Currency_OnChanged);
    }
}
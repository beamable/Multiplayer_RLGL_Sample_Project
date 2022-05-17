using System.Threading.Tasks;
using Beamable;
using UnityEngine;
using UnityEngine.Events;
using Beamable.Common.Api.Inventory;

public class InventoryController : MonoBehaviour
{
    public UnityEvent<InventoryView> OnInventoryUpdated;

    public InventoryView CurrentInventory { get; private set; }
    
    protected IBeamableAPI _beamableAPI;

    /// <summary>
    /// Sets up Beamable, gets the current inventory then subscribes to changes.
    /// <remarks>See <see cref="SetupBeamable"/>, <see cref="GetCurrentInventory"/> and <see cref="SubscribeToInventory"/>.</remarks>
    /// </summary>
    private async void Start()
    {
        await SetupBeamable();
        await GetCurrentInventory();
        SubscribeToInventory();
        Debug.Log(_beamableAPI.User.id);
    }

    /// <summary>
    /// Initializes the Beamable API.
    /// </summary>
    protected async Task SetupBeamable()
    {
        _beamableAPI = await API.Instance;
    }

    /// <summary>
    /// Downloads the user's current inventory and caches it in this component.
    /// </summary>
    protected async Task GetCurrentInventory()
    {
        var inventory = await _beamableAPI.InventoryService.GetCurrent();
        UpdateInventory(inventory);
    }

    /// <summary>
    /// Sets the inventory cached on this component, and invokes the <see cref="OnInventoryUpdated"/> callback.
    /// </summary>
    /// <param name="inventory"></param>
    protected void UpdateInventory(InventoryView inventory)
    {
        CurrentInventory = inventory;
        OnInventoryUpdated?.Invoke(CurrentInventory);
    }
    
    /// <summary>
    /// Subscribes to changes to the user's inventory.
    /// </summary>
    protected void SubscribeToInventory()
    {
        _beamableAPI.InventoryService.Subscribe(UpdateInventory);
    }
}
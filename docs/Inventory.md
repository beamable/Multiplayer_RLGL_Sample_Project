# Inventory

### In-Game Usage

The customize page displays items from the player's inventory. The head tab loads all of the head items in the player's inventory and the body tab loads all of the body items in the player's inventory. If there are no items of that type in the player's inventory, the item area will display text notifying the player, along with a button that takes them to the shop. Clicking on an item will display it on the character as a preview. The name and description of the selected item are shown in a container below the character along with an "equip" button. Using the equip button will save that item as being on the character through the use of Beamable's stats feature. There is also a color swatch widget accessed by clicking a button next to the character, which changes the color of either the head item or the body of the character while viewing their respective tabs. Like the store, the inventory page shows how much in-game currency the player has; currencies are used to buy items and can be earned from playing matches and/or purchased from the store.

### Class Breakdown

- **InventoryController** - Gets the player's inventory and subscribes to its changes.
    - Uses Beamable's inventory service to get the current inventory and to subscribe to any changes in the player's inventory.
    - When a change is detected in the player's inventory, the updated inventory is passed on through a Unity event for the StoreMenu so that the items in the player's current inventory will display as owned and for the InventoryMenu to get the player's current inventory in order to build out the UI on the customization page where owned items are listed.

- **InventoryMenu** - Sets up the UI of the customization page.
    - Uses Beamable's InventoryView to get the inventory and the details about each of the items in the inventory.
    - A ListView is used to build list items around each item's details and display them.

- **ColorSwatchMenu** - Sets up the UI of the color options.
    - Uses Beamable's stats to notify through a Unity event which stat should be changed for the color of the head and body.

- **PlayerCustomizationPreview** - Previews the look of a change to the character from selecting an item.
    - Uses Beamable's PlayerOfferView to get the information about which item is going to be previewed on the character.
    - Uses Beamable's ObtainItem to get the assigned stat key property of the item being changed and to send that value through a Unity event to have the character preview the change via changing stats and applying its value to the item's category.

- **CharacterCustomizationLobby** - Gets and changes the values of the player's stats to change what the character looks like.
    - Uses Beamable's stats to get the player's stats to find which item or color they have selected to equip or preview and to change those stats when a different item or color is selected.
    - Assets that are put on the character are assigned a CharacterCustomizationAsset in the inspector, where the mesh renderer, color, and assigned Beamable ID of the asset are also assigned. There are two lists for these: one that contains head assets and one for body assets.
    - The Beamable ID of the CharacterCustomization asset is used to get the assigned stat for the asset to keep track of which asset the player currently has equipped for the head and body. The same goes for the color of head and body assets.
    - Additionally, the PlayerCustomizationCategory contains the ID and stat object of which stat is to be changed when a new item is selected to be previewed or equipped, or when a new color is selected. These also have a Unity event for when the stat is changed to make sure to apply it to the player's stats only when equipped rather than while being previewed.
    - The preview shows how the stat change would appear for the asset to be displayed on the character and the change is only applied to the stats when the equip button is used. Selecting a color always saves the new color stat for the selected category.


> To learn more about basic usage of the Inventory feature, read about it on Beamable's documentation site [here](https://docs.beamable.com/docs/inventory-feature-overview).

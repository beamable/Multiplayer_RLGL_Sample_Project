# Store

### In-Game Usage

The store page includes category tabs at the top of the item menu to switch the store content. Clicking each category's tab should load the head store (which contains head accessories), the body store, and the gem store which contains a type of currency available to buy with real world money. Selecting an equipable item will display the item on the character model as a preview within the menu. Under the character preview area, there is a container that will display the selected item’s name and description and the buy button. If an item is already in the player’s inventory, the item's icon will display with an alternate version of the price tag; the tag will be disabled, grey, and the word "owned" will replace the price text. The player’s currency balance is displayed in additional containers next to the store tabs. Popups will appear when the player attempts to buy an item to confirm their purchase and then to notify them whether the purchase was successful or if it failed and for what reason.

### Class Breakdown

**StoreMenu** - Sets up the UI of the store.

- Uses Beamable’s player store view to get the listings from the selected store, Beamable’s listing content to get the listings in the store, and Beamable’s player offer view to get the offers in the listings.

- Beamable’s inventory service checks what items the player already owns to compare against the listings so that any listed items cannot be purchased again. This applies to equipable items only; currencies may be purchased multiple times.

**StoreController** - Gets and sets the store from content, handles purchases, and subscribes to changes in the store and in the player’s currency inventory.

- Beamable’s commerce service is used to subscribe to any store changes and to make purchases.

- Beamable’s inventory service is used to subscribe to changes in the player’s currency balance and to check the player's currency amount to make sure they have enough to make the desired purchase.


> To learn more about the Store feature, read more on Beamable's documentation site [here](https://docs.beamable.com/docs/stores-feature-overview).

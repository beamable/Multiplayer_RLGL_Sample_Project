# ListViewComponent

The ListView package is a tool that can be used to dynamically create lists based on supplied data. Each component of the ListView system is virtual, meaning overriding the default functionality for special kinds of lists is supported.

The basic functionality involves setting up a ListViewComponent on a parent object, then assigning a card prefab to that component. The card prefab must have (or be a child class of) a ListCard component. Instances of the card prefab will be spawned under a specified `content` Transform, then the ListViewComponent will pass the data about each card to their respective GameObjects in the list.

### In-Game Usages

The ListView is used in multiple places across the project, namely to build dynamic lists with some data rendered on each "card".

- **Store** - Used to build the grid of store items available for purchase, displaying the item name, price, owned status, and image.
- **Inventory** - Used to build the grid of items in the player's inventory. The data displayed is similar to the Store, with the item name and image.
- **Leaderboards** - Used to build the list of leaderboard entries, with their gamertag, score and rank.

All of the data displayed on the individual cards is passed down through the ListView's pipeline. These are just a few examples among many others that use the ListViewComponent's functionality.

### Class Breakdown

**ListViewComponent** - *Contains logic for spawning instances of a given prefab.*
- `content` (Transform): The parent object, from which prefabs will be spawned as children.
- `prefabs` (List<GameObject>) The list of prefabs this component is able to spawn. See ListPrefabIndex below for more detail.
- `Build(ListViewData data, Action postRefreshAction = null)`: Destroys all objects that are children of `content`, then spawns new card prefab instances based on the provided ListViewData. If a `postRefreshAction` is provided, it will be invoked after the list is finished instantiating all the cards.

**ListCard** - *Contains basic info and functions regarding individual cards.*
- `SetUp(ListItem item)`: A function with no defined behavior in the base class. This is because it is expected to be overridden for custom behavior. Called by the ListViewComponent. Data is to be carried in the ListItem parameter.
- `SetTitle(string title)`: Sets the title text element of the card. The TMP element is checked for null before usage.
- `AddButtonListener(UnityAction action)`: Adds a listener to the viewButton on the card component. The Button element is checked for null before usage.

**ListItem** - *A model containing info about a list item.*
- `ListPrefabIndex:` An index representing which prefab should be spawned. The list of prefabs is contained on the ListViewComponent.
- `Id` and `Title`: strings representing basic data about the card.
- `PropertyBag`: a Dictionary<string, object> used to contain custom data about the card.
- `ViewAction`: An action to be executed when the viewButton is pressed.

**ListViewData** - *Shorthand for a* `List<ListItem>`.

### Intended Workflow

There are a few prerequisites to ensuring the ListViewComponent can access the proper scripts to build a list successfully:
- A card prefab must be created, with either the ListCard component or a child of the ListCard class, attached to the GameObject.
- The ListViewComponent must be added to a parent object (or otherwise an object that will not be deleted), with a card prefab assigned.
- The ListViewComponent must have the `content` Transform assigned, where the card prefabs will be spawned as child instances.
> Layout Groups work nicely for formatted lists. A common practice in the project is to use a Vertical Layout Group along with a Scroll Rect to create dynamic scrollable lists.

Once your hierarchy is set up based on the above steps in the Unity project, the data must be pulled and passed down to the ListViewComponent via scripting.

### Case Study

Let's say we want to build a list of store items. Each item contains a title, description, and price.

```csharp
public class StoreItem
{
    public string title;
    public string description;
    public int price;
}
```

Assuming the data is stored as a `List<StoreItem>`, we can iterate through each item in the list and add it to a new ListViewData. Using the ListViewData, we can call `Build()` on the ListViewComponent.

```csharp
public void BuildList(List<StoreItem> items)
{
    var cardData = new ListViewData();
            
    foreach(var item in items)
    {
        cardData.Add(new ListItem()
        {
            // Title is a built-in string
            Title = item.title,
            
            // Represents the index in the ListViewComponent's list of prefabs
            ListPrefabIndex = 0,
            
            // PropertyBag contains any additional data for custom logic
            // These will need to be deserialized in the card component later
            PropertyBag = new Dictionary<string, object>()
            {
                { "description", item.description },
                { "price", item.price }
            }
        });
    }

    listViewComponent.Build(cardData);
}
```

That is enough to handle building a list with some data. However, actually using the data requires building a subclass of `ListCard` that can make use of the data in a meaningful way.

```csharp
public class StoreCard : ListCard
{
    [SerializeField]
    private TextMeshProUGUI description;
    [SerializeField]
    private TextMeshProUGUI priceText;
```
The important function to override here is the SetUp function, which is called automatically during the ListViewComponent's build process.
```csharp
    public override void SetUp(ListItem item)
    {
        description.text = item.PropertyBag["description"].ToString();
        priceText.text = $"{item.PropertyBag["price"]} Coins";        
    }
}
```

With all of these steps complete, you should have a ListViewComponent fully set up to receive data and draw it dynamically.

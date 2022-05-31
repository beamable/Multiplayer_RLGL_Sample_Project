# Stats

### In-Game Usage

The player can navigate to their profile screen by clicking on their profile icon as displayed in the top left of the lobby screen. The profile screen contains the player's profile icon and gamer tag on the top left of the page, while some player metrics are shown on the right of the screen and achievements are listed on the left. All of this information is stored in Stats.

- Setting the profile name from the EDIT PROFILE button will set the `alias` stat.
- Changing the profile icon will update the `avatar` stat.
- As the player completes matches, various pieces of data about the player will be written to Stats, such as `games_played`, `total_kills`, and so on.

In addition to this info, the list of achievements also uses Stats. As described in the [Achievements](./Achievements.md) documentation, stats are used to drive achievement requirements. Each achievement has a list of stat keys and desired values. When the stats of the player are updated to fulfill this criteria, the achievement is granted.

Stats that are relevant to gameplay, as a result of the player's actions, are written using the BeamableStatsController, which is a wrapper around Beamable's StatsService.

### Class Breakdown

**BeamableStatsController** - A helper class to allow other classes to write stats with simpler syntax.

- This functions as a wrapper around Beamable's StatsService to reduce code duplication.
- Once initialized, there are two main functions to be called: 
  -  `ChangeStat` - Adds or updates the value of a given key-value pair, where the value is a string.
  ```csharp
  _stats = await _beamableAPI.StatsService.GetStats(DOMAIN, ACCESS, STAT_TYPE, _beamableAPI.User.id);
  var newStats = new Dictionary<string, string>
  {
      {key, value}
  };
  await _beamableAPI.StatsService.SetStats(ACCESS, newStats);
  ```
  -  `AddToStat` - Treats the value of the stat like an integer and adds the amount to the value.
  ```csharp
  _stats = await _beamableAPI.StatsService.GetStats(DOMAIN, ACCESS, STAT_TYPE, _beamableAPI.User.id);
  _stats.TryGetValue(key, out var value);
  value ??= "0";
  var newAmount = int.Parse(value) + amount;
  var newStats = new Dictionary<string, string>
  {
      {key, newAmount.ToString()}
  };
  await _beamableAPI.StatsService.SetStats(ACCESS, newStats);
  ```
- Both of these functions save the value to the server when they are called.

**PlayerUsernameUI** and **PlayerIconUI** - Sets up the UI for the player's username text and avatar.

- These two scripts only contain helper functions for setting UI. These functions are public and are called via a UnityEvent from the PlayerStatInfo script.
```csharp
public class PlayerUsernameUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI username;

    public void SetUsername(string alias)
    {
        username.text = alias;
    }
}
```

**PlayerStatInfo** - Responsible for grabbing the player's user ID, avatar, and stats.

- Pulls the data from the server, then invokes various UnityEvents for transferring this data to other objects without creating hard dependencies.
```csharp
//Helper function used in multiple places to simplify syntax
private async Task SetPlayerStat(string statKey, string value, string domainOverride = "client")
{
    Dictionary<string, string> setStats = new Dictionary<string, string>(){{statKey, value}};
    await _context.Api.StatsService.SetStats("public", setStats);
    _stats = await _context.Api.StatsService.GetStats(domainOverride, "public", "player", _userId);
}

//Called by a UI Button
public async void SetUsername(string alias)
{
    await SetPlayerStat(aliasStat.StatKey, alias);
    GetUsername();
}

//Other GameObjects listen to this UnityEvent
public async void GetUsername()
{
    OnGetUsername?.Invoke(await GetPlayerStat(aliasStat.StatKey) ?? aliasStat.DefaultValue);
}
```

**AccountStatsMenu** - Useful for building a [ListView](./ListView.md) of various stats.

- Contains a list of stats and a display name for each, eliminating the need to display the stat's key directly.
```csharp
[Serializable]
public struct NamedStat
{
    public string statKey;
    public string name;
}
[SerializeField]
private List<NamedStat> namedStats;
```
- When enabled, this component builds a ListView with the stat and its current value. Each stat's value is pulled during this function's execution, ensuring it is the latest change.
```csharp
private async Task GetAllStats()
{
    await _context.Api.StatsService.SetStats("public", new Dictionary<string, string>());
    _stats = await _context.Api.StatsService.GetStats("client", "public", "player", _userId);
}

public async void UpdateStatsList()
{
    await GetAllStats();
    var cardData = new ListViewData();
    foreach (var stat in namedStats)
    {
        cardData.Add(new ListItem
        {
            Title = stat.name,
            PropertyBag = new Dictionary<string, object>()
            {
                {"amount", GetPlayerStat(stat.statKey) ?? "0"}
            }
        });
    }
    statsListView.Build(cardData);
}
```

**StatsListCard** - A child class of ListCard that sets a text element with the stat key. This is used in conjunction with AccountStatsMenu.

> For more information on how the ListCard component works, see the [ListView](./ListView.md) documentation.
```csharp
public override void SetUp(ListItem item)
{
    //Virtual function in the base class
    SetTitle(item.Title);
    
    if (item.PropertyBag.ContainsKey("amount"))
    {
        numberLabel.text = item.PropertyBag["amount"] as string;
    }
}
```

> To learn more about basic usage of the Stats feature, read more on Beamable's documentation site [here](https://docs.beamable.com/docs/stats-feature-overview).
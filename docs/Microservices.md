# Microservices

### In-Game Usages
Beamable's microservices are used to send the results of a game in order to calculate scores for rewards, leaderboards, and achievement conditions. When a match ends, the post game results service is called to process and post the results of the game and update the player's stats, grant rewards, and update the leaderboard. The achievement service is called to check progress toward achievement requirements, whether achievement conditions have been met, and to save that information into cloud save.

### Class Breakdown

**ProcessPostGameResults** - Processes the results at the end of a match.
The main function to note here is the ProcessResults function. This is the only client-callable method, and it is the starting point where all tasks relating to game results are ran.
```csharp
[ClientCallable]
public async Task<PostGameResult> ProcessResults(int score, int finishPosition, int totalKills)
```
All of the rewards given to the player, as well as their score and rank, are tracked in an instance of the GameResult class. This will be returned to the client at the end of the function to show a summary in the UI.

First, the game settings are retrieved from the realm config, so we can later access the drop rate for items, and the type of currency awarded.
```csharp
private async Task GetSettingsFromRemoteConfig()
{
    var configSettings = await this.Services.RealmConfig.GetRealmConfigSettings();
    var config = configSettings["gameSettings"];
    _dropRate = int.Parse(config.GetSetting("dropRate"));
    _coins = config.GetSetting("coinType");
}
```
We must also retrieve the "legend" for the points awarded. This also comes from the realm config, but it contains a default value in case the game settings are not found or are unavailable. The legend indicates how many points a player is awarded, based on the order in which they finished the game.
```csharp
private async Task<List<int>> GetLegend()
{
    var configSettings = await this.Services.RealmConfig.GetRealmConfigSettings();
    var config = configSettings["gameSettings"];
    var rankLegend = config.GetSetting("rankLegend");
    BeamableLogger.Log($"Rank Legend: {rankLegend}");
    var legend = JsonUtility.FromJson<List<int>>(rankLegend);
    return legend.Count == 0 ? new List<int>() {85, 70, 55, 40, 25, 10, -5, -15} : legend;
}
```
The final value in the sequence indicates the lowest possible score difference that a player can be awarded. Using the default legend on the last line, we can see that the values should be awarded as such:
```
1st place: 85 points
2nd place: 70 points
3rd place: 55 points
...
8th place or lower: -15 points
```
> The points system used here is called "Elo", a common multiplayer game ranking system. For more information about the Elo rating system, read more on its [Wikipedia page](https://en.wikipedia.org/wiki/Elo_rating_system).

After the score is calculated, the final game results are are calculated and saved via the `SetRanks` method. The function will grant 2 rank points for every kill, 1 extra rank point for every 100 score, among other small adjustments.

*This function has been paraphrased slightly for readability.*
```csharp
var newRank = positionScore + previousRank;
_gameResult.OldRank = previousRank;
_gameResult.NewRank = newRank;
_gameResult.GamesPlayed = gamesPlayed + 1;

//Grant 2 points for every kill.
if (totalKills > 0)
{
    for (var i = 0; i < totalKills; i++)
    {
        _gameResult.NewRank += 2; 
    }
}

//Grant 1 extra rank point per 100 score.
for (var i = 0; i < score; i += 100)
{
    _gameResult.NewRank++;
}
```
Next, the player will be granted some currency and item rewards. These are handled by the `GrantCurrencyRewards` and `GrantItemRewards` functions, respectively. The following awards 100 currency for finishing a match, with an additional 100 currency for finishing in the top 8 places.
```csharp
private async Task GrantCurrencyRewards(int finishPosition)
{
    await Services.Inventory.AddCurrency(_coins, 100);
    _gameResult.Rewards.Add(new Reward()
    {
        IsItem = false,
        Amount = finishPosition > 8 ? 100 : 200,
        CurrencyType = _coins,
        ItemContentId = string.Empty
    });
}
```
The player then has a chance to earn a new Head accessory. The drop rate is a percentage defined in the realm config settings.

*This function has been paraphrased slightly for readability.*
```csharp
//Get the drop rate from the realm config.
_dropRate = int.Parse(config.GetSetting("dropRate"));
//Pull the store content. Store is a constant string equal to "stores.HeadStore".
var headStore = await Services.Content.GetContent(Store, typeof(StoreContent));
var headStoreRef = headStore as StoreContent;
var listings = headStoreRef.listings;
//Calculate a random integer from 0 to 100.
var rnd = new Random();
var val = rnd.Next(0, 100);
//Don't grant the item if the value doesn't land within the drop rate.
if (val >= _dropRate) return;
//Get a random item from the store.
var itemIndex = itemRnd.Next(0, listings.Count);
var item = await listings[itemIndex].Resolve();
//Since we only use 1 obtain item per content object, we can get the ID at the first index.
var itemContentId = item.offer.obtainItems[0].contentId;
//Add the items to the rewards list for the user.
_gameResult.Rewards.Add(new Reward()
{
    IsItem = true,
    ItemContentId = itemContentId
});
```
After calculating rewards, the leaderboard score and stats are uploaded to the server. If the user has not set an alias, "Anonymous" is used as a failsafe.
```csharp
var statsToPost = new Dictionary<string, object>()
{
    {GamesPlayed, _gameResult.GamesPlayed},
    {Rank, _gameResult.NewRank},
    {Score, score},
    {Kills, totalKills},
    {Alias, stats.ContainsKey(Alias) ? stats[Alias] : "Anonymous"}
};
//Access is a constant string equivalent to "public".
await this.Services.Stats.SetStats(Access, statsToPost);
```
The leaderboard score is posted here. For this sample project, the maximum size for a leaderboard is 100 entries, but this can be tweaked for the project's requirements.
```csharp
var currentLb = await this.Services.Leaderboards.GetBoard(LeaderboardName,1,100);
if (currentLb != null)
{
    await this.Services.Leaderboards.SetScore(LeaderboardName, (double) _gameResult.NewRank, newStats);
}
```
After all these steps are finished, the results of the match are sent back to the client to be displayed in the UI. This can be found in the `PlayerCharacter` script.
```csharp
result = await postGameResultsClient.ProcessResults(gameScore, finishedPosition, totalKills);
```

**AchievementsService** - Checks if achievement requirements have been met, and awards relevant achievements to the user.
- The Content service is used to load the content from an achievement group.
- The Stat service is used to check the content against the corresponding stats and to return a list of achievements and whether or not any achievements have been earned.
- On the client side, Cloud Save is used to save the achievements for the user's account.

> The full documentation for the Achievements microservice can be found in the [Achievements documentation](./Achievements.md).


> To learn more about the Mircoservices feature, read more on Beamable's documentation site [here](https://docs.beamable.com/docs/microservices-feature-overview).

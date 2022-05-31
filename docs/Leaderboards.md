# Leaderboard

### In-Game Usage

At the end of each *Red Light, Green Light* match, the player will get to see their position in the global leaderboards. The player's position is determined by their performance in the previous match, combined with their performance in all previously played matches. The player's score is calculated based on two point values: 
1. Player kills = A configurable point value.
2. Time player reached the finish line = A configurable point value determined on a linear scale based on the total time of the match. 

Both of these point values can be configured independently.

### Prefabs

- LeaderboardUI - Contains the implementation of the Leaderboard Controller and all instances of ScoreCard Prefabs.
- ScoreCard - Contains the references to the visual aspects of each user; for example, name, score, and rank.

### Class Breakdown

**LeaderboardController** - Handles the GUI shown at the end of each match and in the Main Menu when displaying the Leaderboard.

- Uses Beamable's Leaderboard service to get the leaderboard wanted to display.

- Uses Beamable's [Stats](./Stats.md) service to get and display the players' relavent information.

- Uses a [ListView](./ListViewComponent.md) to display each user in their ranked order.
```csharp
public async void GetLeaderboard()
{
    //_leaderboardId is a string equivalent to the ID of the leaderboard we want to get. Since there is only one leaderboard used in the project, this is a constant.
    _leaderBoardView = await _context.Api.LeaderboardService.GetBoard(_leaderboardId, 0, 100, _context.PlayerId);
    UpdateList();
}

public async void UpdateList()
{
    var cardData = new ListViewData();

    foreach (var rankEntry in _leaderBoardView.rankings)
    {
        var stats = await _context.Api.StatsService.GetStats("client", "public", "player", rankEntry.gt); //gt is gamertag
        var alias = stats.ContainsKey("alias") ? stats["alias"] : "Anonymous";
        var entryStats = rankEntry.stats;
        cardData.Add(new ListItem
        {
            Title = alias,
            PropertyBag = new Dictionary<string, object>()
            {
                {"rank", rankEntry.rank},
                {"score", rankEntry.score},
                {"current_player", rankEntry.gt == BeamContext.Default.PlayerId}
            },
            ListPrefabIndex = 0
        });
        if (rankEntry.gt == BeamContext.Default.PlayerId)
        {
            //Footer shows the current player their own score and ranking on the leaderboard
            footerCard.SetUp(cardData[cardData.Count-1]);
        }
    }

    listViewComponent.Build(cardData);
}
```

**ScoreCard** - Holds references to the UnityGUI elements used to display each user's name, score, rank, etc.

- Updates the Background image based on if the player is the current user.
```csharp
private void SetBackground(bool isCurrentPlayer)
{
    if (isCurrentPlayer)
    {
        if (currentPlayerBackground != null)
        {
            background.sprite = currentPlayerBackground;
        }
    }
    else
    {
        if (genericPlayerBackground != null)
        {
            background.sprite = genericPlayerBackground;
        }
    }
}
```


> To learn more about basic usage of the Leaderboard feature, it can be found on Beamable's docs [here](https://docs.beamable.com/docs/leaderboards-code).

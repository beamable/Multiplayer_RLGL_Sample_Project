# Microservices

### In-Game Usages
Beamable's microservices are used to send the results of a game in order to calculate scores for rewards, leaderboards, and achievement conditions. When a match ends, the post game results service is called to process and post the results of the game and update the player's stats, grant rewards, and update the leaderboard. The achievement service is called to check progress toward achievement requirements, whether achievement conditions have been met, and to save that information into cloud save.

### Class Breakdown

**ProcessPostGameResults** - Processes the results at the end of a match.
- Stat service is used to update relevant stats.
- Realm config service gets the legend for calculating score and determines which rewards to give.
- Inventory service and content service are used to grant rewards.
- Leaderboard service is used to update the leaderboard with the calculated score.

**AchievementsService** - Checks if achievement requirements has been met.
- Content service is used to load the content from an achievement group.
- Stat service is used to check the content against the corresponding stats and to return a list of achievements and whether or not any achievements have been earned.


> To learn more about the Mircoservices feature, read more on Beamable's documentation site [here](https://docs.beamable.com/docs/microservices-feature-overview).

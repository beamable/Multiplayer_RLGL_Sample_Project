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

**ProcessPostGameResults** - Processes the results at the end of a match.

- Stat service is used to update related stats.

- Realm config service is used to get the legend for calculating score and for getting what rewards to give.

- Inventory service and content service are used to give rewards.

- Leaderboard service is used to update the leaderboard with the calculated score.

**LeaderboardController** - Handles the GUI shown at the end of each match and in the Main Menu when displaying the Leaderboard.

- Uses Beamable's leaderboard service to update the leaderboard with the calculated score.

- Displays each user in their ranked order.

**ScoreCard** - Holds references to the UnityGUI elements used to display each user's name, score, rank, etc.

- Updates the Background image based on rank.


> To learn more about basic usage of the Leaderboard feature, it can be found on Beamable's docs [here](https://docs.beamable.com/docs/leaderboards-code).

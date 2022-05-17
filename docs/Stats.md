# Stats

### In-Game Usage

The player can navigate to their profile screen by clicking on their profile icon as displayed in the top left of the lobby screen. The profile screen contains the player's profile icon and gamer tag on the top left of the page, while some player metrics are shown on the right of the screen and achievements are listed on the left. All of this information is stored in Stats.

- Setting the profile name from the EDIT PROFILE button will set the `alias` stat.
- Changing the profile icon will update the `avatar` stat.
- As the player completes matches, various pieces of data about the player will be written to Stats, such as `games_played`, `total_kills`, and so on.

In addition to this info, the list of achievements also uses Stats. As described in the [Microservices](./Microservices.md) documentation, stats are used to drive achievement requirements. Each achievement has a list of stat keys and desired values. When the stats of the player are updated to fulfill this criteria, the achievement is granted.

Stats that are relevant to gameplay, as a result of the player's actions, are written using the BeamableStatsController, which is a wrapper around Beamable's StatsService.

### Class Breakdown

**BeamableStatsController** - A helper class to allow other classes to write stats with simpler syntax.

- This functions as a wrapper around Beamable's StatsService to reduce code duplication.
- Once initialized, there are two main functions to be called: 
  -  `ChangeStat` - Adds or updates the value of a given key-value pair, where the value is a string. 
  -  `AddToStat` - Treats the value of the stat like an integer and adds the amount to the value.
- Both of these functions save the value to the server when they are called.

**PlayerUsernameUI** and **PlayerIconUI** - Sets up the UI for the player's username text and avatar.

- These two scripts only contain helper functions for setting UI. These functions are public and are called via a UnityEvent from the PlayerStatInfo script.

**PlayerStatInfo** - Responsible for grabbing the player's user ID, avatar, and stats.

- Pulls the data from the server, then invokes various UnityEvents for transferring this data to other objects without creating hard dependencies.

**AccountStatsMenu** - Useful for building a [ListView](./ListView.md) of various stats.

- Contains a list of stats and a display name for each, eliminating the need to display the stat's key directly.
- When enabled, this component builds a ListView with the stat and its current value. Each stat's value is pulled during this function's execution, ensuring it is the latest change.

**StatsListCard** - A child class of ListCard that sets a text element with the stat key.

- For more information on how the ListCard component works, see the [ListView](./ListView.md) documentation.

> To learn more about basic usage of the Stats feature, read more on Beamable's documentation site [here](https://docs.beamable.com/docs/stats-feature-overview).

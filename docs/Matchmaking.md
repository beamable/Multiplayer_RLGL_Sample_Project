# Matchmaking

### In-Game Usage

From the lobby screen, the "Find Matchmaking" button kicks off a search to find eligible players to join a game. The matchmaking is driven by a Game Type, which dictates how many players need to be found for a match, how long to wait after the minimum player count is reached, etc. These parameters are listed in more detail on Beamable's Matchmaking [Content Setup](https://docs.beamable.com/docs/matchmaking-guide#setup-content) guide.

### Class Breakdown

**MatchmakingHandler** - Responsible for calling Beamable MatchmakingService APIs.
- Has functions named `StartMatchmaking` and `CancelMatchmaking`, which starts and stops a search, respectively.
- Contains various callbacks for the Matchmaking system, including when the match is ready, when the player count is updated, or when the search times out.

**SceneLoader** - A basic wrapper around Unity's SceneManager.
- Contains public functions for loading scenes by index, or by name.
- Contains the OnProgressUpdated event, which reports the scene's load progress. This be attached to a loading bar.

**MatchConfig** - A ScriptableObject containing information about the match.
- The match ID is stored on this object when a match is found, so it can persist between scenes.

**FusionLauncher** - Handles Player Joining, Player Leaving, and all other basic Fusion Callbacks, such as Connection Request and Fusion Runner initialization.
- Reads the match ID from the MatchConfig object and joins into the Photon room of the same ID.
- Further documented on the (Photon Fusion)[./Photon Fusion.md] documentation.

> To learn more about the Matchmaking feature, read more on Beamable's documentation site [here](https://docs.beamable.com/docs/matchmaking-feature-overview).

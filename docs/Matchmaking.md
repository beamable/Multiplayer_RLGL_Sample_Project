# Matchmaking

### In-Game Usage

From the lobby screen, the "Find Matchmaking" button kicks off a search to find eligible players to join a game. The matchmaking is driven by a Game Type, which dictates how many players need to be found for a match, how long to wait after the minimum player count is reached, etc. These parameters are listed in more detail on Beamable's Matchmaking [Content Setup](https://docs.beamable.com/docs/matchmaking-guide#setup-content) guide.

### Class Breakdown

**MatchmakingHandler** - Responsible for calling Beamable MatchmakingService APIs.

This script contains `StartMatchmaking` and `CancelMatchmaking` functions, which start and stop a match search, respectively. These functions contains various callbacks for the Matchmaking system, including when the match is ready, when the player count is updated, when an error occurs, or when the search times out.
```csharp
//Get the data for the game type used for matchmaking
var gameType = await gameTypeRef.Resolve();

var handle =  await _context.Api.Experimental.MatchmakingService.StartMatchmaking(gameTypeRef, UpdateHandler, ReadyHandler, TimeoutHandler).Error(error =>
{
    //LogToOutput writes a string to a TextMeshPro object. It is not necessary for the script to function.
    LogToOutput("Matchmaking failed to start search!");
    LogToOutput(error.Message);
    LogToOutput(error.StackTrace);
});
//Save the ticket ID so we can cancel it later, if we need to.
_currentTicketId = handle.Tickets.First().ticketId;
```
Each handler responds to the match state by invoking an event that other objects can subscribe to, among other coupled functionality. For example, take the ReadyHandler as an example:
```csharp
private void ReadyHandler(MatchmakingHandle handle)
{
    LogToOutput(handle.State.ToString());
    LogToOutput(JsonUtility.ToJson(handle.Match));
    TrackEvent(Time.time - _startTime, handle.Match.matchId);
    OnMatchReady?.Invoke(handle.Match.matchId);
}
```
Other than logging some debug information and writing telemetry, the responsibility of this function is to invoke the OnMatchReady UnityEvent, and pass the match ID. In this particular sample, the match ID is written to a ScriptableObject (the MatchConfig, described in more detail below). This is later used to join a Photon room of the same ID.

**SceneLoader** - A basic wrapper around Unity's SceneManager.
- Contains public functions for loading scenes by index, or by name.
- Contains the OnProgressUpdated event, which reports the scene's load progress. This be attached to a loading bar.
- This script only allows scenes to be loaded asynchronously (via `SceneManager.LoadSceneAsync`). Therefore, a loading screen should be shown in the current scene while the next scene is loading.

**MatchConfig** - A ScriptableObject containing information about the match.
- The match ID is stored on this object when a match is found, so it can persist between scenes.
- When the FusionInitializer scene is loaded, the match ID is pulled from the config and attempts to join the Photon room with that ID. Whether or not the room already "exists" is irrelevant.
- If the match ID is blank or a default value, the FusionInitializer scene will join a "default" room, so the game can be tested locally without needing to go through the matchmaking process.

**FusionLauncher** - Handles Player Joining, Player Leaving, and all other basic Fusion Callbacks, such as Connection Request and Fusion Runner initialization.

The highlight of this script is the `StartGame()` function, which is called from the `Launch()` function:

```csharp
await _runner.StartGame(new StartGameArgs()
{
    GameMode = mode, 
    SessionName = matchConfig == null ? room : matchConfig.MatchId,
    SceneObjectProvider = sceneLoader,
    PlayerCount = playerCount
});
```
This displays the previously mentioned "default" match ID.

Another important function to note is the `OnShutdown` callback. To maintain clean disconnect practices, it is important to ensure a clean break from the server when a player leaves a match, and to ensure there are no hanging subscriptions or otherwise negatively impacting the other players who are still connected.
```csharp
public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
```
Inside this function, there are multiple "cleanup tasks" that must be run to ensure a smooth user experience:
```csharp
//Destroy all NetworkObjects
NetworkObject[] nos = FindObjectsOfType<NetworkObject>();
for (int i = 0; i < nos.Length; i++)
    Destroy(nos[i].gameObject);

//Destroy the NetworkRunner instance
if (_runner != null && _runner.gameObject != null)
    Destroy(_runner.gameObject);

//Make sure the cursor is visible
Cursor.lockState = CursorLockMode.None;
Cursor.visible = true;
```
Outside of the functions described here, many of Fusion's callbacks go unused. This is because FusionLauncher inherits from `INetworkRunnerCallbacks`, which is a relatively lengthy interface.

> Information about Fusion is further documented on the [Photon Fusion](./Photon%20Fusion.md) documentation.

> To learn more about the Matchmaking feature, read more on Beamable's documentation site [here](https://docs.beamable.com/docs/matchmaking-feature-overview).

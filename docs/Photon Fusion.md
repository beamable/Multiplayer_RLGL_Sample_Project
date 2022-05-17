# Photon Fusion

### In-Game Usage

Fusion is a new high performance state synchronization networking library for Unity. With a single API, it supports many network architectures, including dedicated server, client hosted, and shared/distributed authority. **This example uses the client hosted model.**

### How to Initialize

In our demo, after we gather all players from Beamable's matchmaking service, we load the FusionInitializer Unity Scene. This scene contains all the required Fusion Initialization components. The entry point is the GameLauncher class. This class then creates an instance of the FusionInitialzer to handle much of the basic Fusion Runner initialization. From here, the GameLauncher then creates an instance of the GameManger. The GameMananger handles some basic game management and helps with loading in Unity Scene through the LevelManager component.

### Scenes Breakdown

- FusionInitializer - Starting scene that launches a Fusion Runner and holds unique scene components, such as the Cameras and Player Input Listener.

- Level_RedLight - Game Scene. The player must avoid being spotted by the "Evil Eye" while also avoiding or actively attacking other players, all the while trying to reach the center of the level before other players.

### Class Breakdown

**GameLauncher** - Handles basic room creation and joining. It also handles Fusion Launch Callbacks for spawing a world and player.

- Adds the FusionLauncher component to create a Fusion Instance and listen to relevant callbacks.

- Updates a simple UI to Host or Join a game instance.

- Spawns in a networked GameManager which controls basic game state.

**FusionLauncher** - Handles Player Joining/Leaving, and all other basic Fusion Callbacks, such as Connection Request and Fusion Runner initialization.

- Instantiates a Fusion Runner instance and creates a game.

**GameManager** - This class holds the overall game state and handles some basic game logic; for example, what to do once all players have readied up.

- Listens for the escape key; if used, it will disconnect the player from the Fusion instance.

- Holds a reference to the LevelManager.

**LevelManager** - This class is Fusion NetworkSceneManagerBase and handles all network scene loading and scene unloading. We also use this class to start a "level" which in our case is the *Red Light, Green Light* game. This is handled in the RedLightManager. This class is passed into the FusionLauncher Launch function where it is used as the Runners SceneObjectProvider.

- Asynchronously loads in Unity Scenes and handles any NetworkObject Spawning. In this case, we spawn in networked weapon spawners.

- Starts the *Red Light, Green Light* gameplay.

**RedLightManager** - This class handles the core gameplay loop of the *Red Light, Green Light* mechanic.

- Grabs Beamable Content, which holds several gameplay variables, including game length and score values.

- Checks for player movement when in Red Light Mode. If motion is found, this class will initialize the killing of the found player.

- Checks for game completion based on several inputs, such as player reaching the finish line and player death.

- Updates each player to the state of the *Red Light, Green Light* mechanic.

**PlayerCharacter** - This class is handles various aspects of the player.

- Networked movement.

- Networked Player stats, including health and score.


> To learn more about basic usage of the Photon Fusion, read more [here](https://doc.photonengine.com/en-us/fusion/current/getting-started/fusion-intro).

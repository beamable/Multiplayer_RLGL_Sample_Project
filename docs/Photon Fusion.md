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
```csharp
FusionLauncher launcher = FindObjectOfType<FusionLauncher>();
if (launcher == null)
{
    launcher = new GameObject("Launcher").AddComponent<FusionLauncher>();
}
launcher.Launch(_gameMode, _room.text, lm, _matchConfig, OnConnectionStatusUpdate, OnSpawnWorld, OnSpawnPlayer, OnDespawnPlayer);
```

- Updates a simple UI to Host or Join a game instance.

- Spawns in a networked GameManager which controls basic game state.
```csharp
runner.Spawn(_gameManagerPrefab, Vector3.zero, Quaternion.identity, null, InitNetworkState);
void InitNetworkState(NetworkRunner runner, NetworkObject world)
{
    world.transform.parent = transform;
}
```

**FusionLauncher** - Handles Player Joining/Leaving, and all other basic Fusion Callbacks, such as Connection Request and Fusion Runner initialization.

These callbacks come from the INetworkRunnerCallbacks interface, which FusionLauncher implements. These are called automatically by Photon.
```csharp
public void OnConnectedToServer(NetworkRunner runner)
{
    Debug.Log("Connected to server");
    if (runner.GameMode == GameMode.Shared)
    {
        Debug.Log("Shared Mode - Spawning Player");
        InstantiatePlayer(runner, runner.LocalPlayer);
    }
    SetConnectionStatus(ConnectionStatus.Connected, "OnConnectedToServer");
}

public void OnDisconnectedFromServer(NetworkRunner runner)
{
    Debug.Log("Disconnected from server");
    SetConnectionStatus(ConnectionStatus.Disconnected, "OnDisconnectedFromServer");
}
```

- Instantiates a Fusion Runner instance and creates a game.
```csharp
if(_runner == null)
{
    _runner = gameObject.AddComponent<NetworkRunner>();
    _runner.name = name;
    _runner.ProvideInput = mode != GameMode.Server;
}
```

**GameManager** - This class holds the overall game state and handles some basic game logic; for example, what to do once all players have readied up.

- Listens for the escape key; if used, it will disconnect the player from the Fusion instance.
```csharp
if (_restart || Keyboard.current.escapeKey.wasPressedThisFrame)
{
    Restart(_restart ? ShutdownReason_GameAlreadyRunning : ShutdownReason.Ok);
}
```

- Holds a reference to the LevelManager.

**LevelManager** - This class is Fusion NetworkSceneManagerBase and handles all network scene loading and scene unloading. We also use this class to start a "level" which in our case is the *Red Light, Green Light* game. This is handled in the RedLightManager. This class is passed into the FusionLauncher Launch function where it is used as the Runners SceneObjectProvider.

- Asynchronously loads in Unity Scenes and handles any NetworkObject Spawning. In this case, we spawn in networked weapon spawners.
```csharp
//Loads the scene for all connected players.
public void LoadLevel(int nextLevelIndex)
{
    Runner.SetActiveScene(_levels[nextLevelIndex]);
}
```
An important function to note is the StartLevel function. This starts the *Red Light, Green Light* gameplay.
```csharp
public void StartLevel()
{
    // Start the countdown.
    StartCoroutine(LevelSoundManager.Instance.TransitionToMainLoop(() =>
    {
        _countdownManager = FindObjectOfType<CountdownManager>(true);
        _countdownManager.Reset();

        // Start the countdown.
        StartCoroutine(_countdownManager.Countdown(() =>
        {
            GameManager.playState = GameManager.GameState.LEVEL;

            if (readyupManager != null)
                readyupManager.HideReadyUpUI();

            // Spawn in the weapon pickups.
            if (Runner.IsServer)
                _currentLevel.ActivateSpawners(Runner);

            // Begin the redlight game.
            redLightManager.Reset();
            redLightManager.BeginGame();
        }));
    }));
}
```

**RedLightManager** - This class handles the core gameplay loop of the *Red Light, Green Light* mechanic.
- Updates each player to the state of the *Red Light, Green Light* mechanic.

- Grabs Beamable Content, which holds several gameplay variables, including game length and score values.
```csharp
private async Task ResolveRefs()
{
    _redLightGameType = await _redLightGameTypeRef.Resolve();

    GetFloatRule("MatchTime", out matchTime);

    if (_matchTimer == null)
        _matchTimer = FindObjectOfType<MatchTimer>(true);

    if (_matchTimer != null)
    {
        _matchTimer.SetTimerText(matchTime);
    }

    GetIntRule("MaxSafePlayers", out maxSafePlayers);
    GetIntRule("KillGreenPoints", out _killGreenPoints);
    GetIntRule("KillRedPoints", out _killRedPoints);
    GetIntRule("MaxFinishPoints", out _MaxFinishPoints);
    GetIntRule("MaxFailPoints", out _MaxFailPoints);

    Reset();
}

private void GetFloatRule(string ruleName, out float property)
{
    var rule = _redLightGameType.stringRules.Find(p => p.property == ruleName);
    if (rule != null)
        float.TryParse(rule.value, out property);
    else
        property = 0.0f;
}

private void GetIntRule(string ruleName, out int property)
{
    var rule = _redLightGameType.stringRules.Find(p => p.property == ruleName);
    if (rule != null)
        int.TryParse(rule.value, out property);
    else
        property = 0;
}
```

- Checks for player movement when in Red Light Mode. If motion is found, this class will initialize the killing of the found player. Since this is a fairly lengthy function, only the major sections are covered.
```csharp
private void LineOfSightTest()
{
// We only want to run on the Client/Player with InputAuth.
if (_inputAuthPlayer == null || _inputAuthPlayer.State != PlayerCharacter.PlayerState.Active || _inputAuthPlayer.Object.HasInputAuthority == false)
{
    return;
}

// Get the direction to the from the eye to the player.
//Here, we calculate toPlayerCenter, toPlayerLeft, and toPlayerRight. This gives a more comprehensive check to see if any part of the player is visible.
Vector3 toPlayerCenter = (_inputAuthPlayer.transform.position + Vector3.up * 1.6f) - aimLOC.transform.position;
Vector3 toPlayerLeft = (_inputAuthPlayer.transform.position + Vector3.up * 1.3f + _inputAuthPlayer.transform.right * -_lineOfSightWidth) - aimLOC.transform.position;
Vector3 toPlayerRight = (_inputAuthPlayer.transform.position + Vector3.up * 1.3f + _inputAuthPlayer.transform.right * _lineOfSightWidth) - aimLOC.transform.position;

RaycastHit hitInfo;
int hitCount = 0;

//This type of raycast is duplicated for the player center, left, and right.
if (Physics.Raycast(aimLOC.transform.position, toPlayerCenter, out hitInfo, 1000.0f, _lineOfSightMask, QueryTriggerInteraction.Ignore))
{
    if (hitInfo.collider.gameObject.layer == _inputAuthPlayer.KCC.Collider.gameObject.layer)
    {
        hitCount++;
    }
}

if (hitCount > 1)
{
    PlayerHUDInfoUI.Instance.SetVisibleIconUI(true);
    _inputAuthPlayer.hasBeenSeen = true;
    MotionTest();
}
```
Now that we know the player is visible, we need to do a motion test to see if the player is moving. Again, this function has been shortened to demonstrate the core functionality.
```csharp
//A small tolerance is added so the movement is slightly more forgiving.
if (_lineOfSightTimer > _lineOfSightTolerance)
{
    if (_inputAuthPlayer.KCC.RenderData.RealVelocity.magnitude >= _failThreshold)
    {
        _inputAuthPlayer.RPC_SetDamageInfo(3, GameManager.KillSource.LASER, WeaponType.LASER, DamageModifiers.NONE, PlayerRef.None);

        // Send out the word to kill the player. The player has InputAuth.
        _inputAuthPlayer.RPC_KillPlayer(GameManager.DisplayState.ELIMINATION, GameManager.KillSource.LASER, reason);
    }
}
```

- Checks for game completion based on several inputs, such as player reaching the finish line and player death. This only gets run on the server/host. In this case the "other" collider is the finish line flag.
```csharp
private void OnTriggerEnter(Collider other)
{
    if (!Object.HasStateAuthority)
        return;

    PlayerCharacter player = other.GetComponentInParent<PlayerCharacter>();
    if (player != null)
    {
        if (player.State != PlayerCharacter.PlayerState.Safe && player.State != PlayerCharacter.PlayerState.Dead)
        {
            player.SetPlayerSafe();

            if (_safePlayers.Contains(player) == false)
            {
                _safePlayers.Add(player);
                _safePlayerCount = _safePlayers.Count;
                
                if (_safePlayerCount == 1)
                {
                    player.MarkFirstPlace();
                }
                player.AddToMatchesWon();

                player.gameScore += CalculatePointsForFinish();

                GameCompletionCheck();
            }
        }
    }
}

private void GameCompletionCheck()
{
    if (!Object.HasStateAuthority)
        return;

    int aliveCount = PlayerManager.PlayersAliveCount();
    int deadCount = PlayerManager.PlayersDeadCount();
    int safeCount = PlayerManager.PlayersSafeCount();

    // Game Completion Checks
    if (PlayerManager.PlayersActiveCount() == _safePlayers.Count)
    {
        // All living players have crossed the finish-line.
        StopGame();

    }
    else if (safeCount >= maxSafePlayers)
    {
        // Enough players crossed the finish-line so that we can consider the game completed.
        StopGame();
    }
    else if (aliveCount == 0)
    {
        // All players are dead.
        StopGame();
    }
}
```

**PlayerCharacter** - This class is handles various aspects of the player.

- Networked movement.

Both the look rotation and the player movement are done using Fusion's Kinematic Character Controller (KCC).
```csharp
// Some conditions are met, we can apply pending look rotation delta to KCC.
if (_pendingLookRotationDelta.IsZero() == false)
{
    KCC.AddLookRotation(_pendingLookRotationDelta);
    _pendingLookRotationDelta = default;
}

//inputDirection is grabbed from Unity's input system.
KCC.SetInputDirection(inputDirection);
```

- Networked Player stats, including health and score.
```csharp
private static void OnPlayerHealthChanged(Changed<PlayerCharacter> changed)
{
    // Get the changed value.
    int newHealth = changed.Behaviour.GetPlayerHealth();

    // Example of how to get old and new values.
    changed.LoadOld();
    int prevHealth = changed.Behaviour.GetPlayerHealth();

    //The OnPlayerHealthChanged function only updates UI.
    if (changed.Behaviour)
        changed.Behaviour.OnPlayerHealthChanged(newHealth);
}
```

> To learn more about basic usage of the Photon Fusion, read more [here](https://doc.photonengine.com/en-us/fusion/current/getting-started/fusion-intro).

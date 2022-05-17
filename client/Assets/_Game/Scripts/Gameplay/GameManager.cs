using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Fusion;

namespace BeamableExample.RedlightGreenLight
{
    public class GameManager : NetworkBehaviour, IStateAuthorityChanged
    {
        public const ShutdownReason ShutdownReason_GameAlreadyRunning = (ShutdownReason)100;

        public enum GameState
        {
            LOBBY,
            LEVEL,
            SCOREBOARD,
            TRANSITION
        }

        public enum DisplayState
        {
            ELIMINATION,
            COMPLETION
        }

        public enum KillSource
        {
            PLAYER,
            LASER,
            ENDOFGAME
        }

        [Networked]
        private GameState networkedPlayState { get; set; }

        public static GameState playState
        {
            get => (Instance != null && Instance.Object != null && Instance.Object.IsValid) ? Instance.networkedPlayState : GameState.LOBBY;
            set
            {
                if (Instance != null && Instance.Object != null && Instance.Object.IsValid)
                    Instance.networkedPlayState = value;
            }
        }

        public LevelManager levelManager;

        private bool _restart;
        
        public static GameManager Instance { get; private set; }

        private void Awake()
        {
        }

        public override void Spawned()
        {
            // We only want one GameManager
            if (Instance)
                Runner.Despawn(Object);
            else
            {
                Instance = this;

                // Find managers and UI
                levelManager = FindObjectOfType<LevelManager>(true);

                if (Object.HasStateAuthority)
                {
                    // TODO:: For the demo we are just loading in the redlight scene.
                    // Later we will want to come up with a was of loading different game types/levels.
                    // NOTE:: This is the index of the level in the list of game levels in the LevelManager. This is not the build index of the scene.
                    LoadLevel(0);
                }
                else if (playState != GameState.LOBBY)
                {
                    Debug.Log("Rejecting Player, game is already running!");
                    _restart = true;
                }
            }
        }

        public void Restart(ShutdownReason shutdownReason)
        {
            if (!Runner.IsShutdown)
            {
                // Calling with destroyGameObject false because we do this in the OnShutdown callback on FusionLauncher
                Runner.Shutdown(false, shutdownReason);
                Instance = null;
                _restart = false;
            }
        }

        private void Update()
        {
            if (_restart || Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                Debug.Log("<color=green>GameManager Shutdown Triggerd</color>");
                Restart(_restart ? ShutdownReason_GameAlreadyRunning : ShutdownReason.Ok);
                return;
            }
            PlayerManager.HandleNewPlayers();
        }

        // Transition from lobby to level
        public void OnAllPlayersReady()
        {
            Debug.Log("All players are ready");
            if (playState != GameState.LOBBY)
                return;

            // Close and hide the session from matchmaking / lists. this demo does not allow late join.
            Runner.SessionInfo.IsOpen = false;
            Runner.SessionInfo.IsVisible = false;

            levelManager.StartLevel();
        }

        private void LoadLevel(int nextLevelIndex)
        {
            if (Object.HasStateAuthority)
            {
                // Reset players ready state so we don't start the game immediately
                for (int i = 0; i < PlayerManager.allPlayers.Count; i++)
                    PlayerManager.allPlayers[i].SetReady(false);

                levelManager.LoadLevel(nextLevelIndex);
            }
        }

        public void StateAuthorityChanged()
        {
            Debug.Log($"State Authority of GameManager changed: {Object.StateAuthority}");
        }
    }
}
using Beamable;
using BeamableExample.FusionHelpers;
using BeamableExample.RedlightGreenLight.Character;
using BeamableExample.UI;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace BeamableExample.RedlightGreenLight
{
	/// <summary>
	/// App entry point and main UI flow management.
	/// </summary>
	public class GameLauncher : MonoBehaviour
	{
		[SerializeField] private GameManager _gameManagerPrefab;
		[SerializeField] private PlayerCharacter _playerPrefab;
		[SerializeField] private TMP_InputField _room;
		[SerializeField] private TextMeshProUGUI _progress;
        [SerializeField] private CanvasGroup _uiCurtain;
        [SerializeField] private CanvasGroup _uiStart;
        [SerializeField] private CanvasGroup _uiProgress;
        [SerializeField] private CanvasGroup _uiRoom;

        [SerializeField] private MatchConfig _matchConfig;

		private FusionLauncher.ConnectionStatus _status = FusionLauncher.ConnectionStatus.Disconnected;
		private GameMode _gameMode;
		private bool _loaded;

		private BeamableBehaviour _beamable;

		public UnityEvent OnPlayerSpawned;
		public UnityEvent OnPlayerDespawned;
		private void Awake()
		{
			//DontDestroyOnLoad(this);
			_loaded = false;
		}

		private void Start()
		{
			OnConnectionStatusUpdate(null, FusionLauncher.ConnectionStatus.Disconnected, "");

			// TODO:: Maybe a better way to do this. Should Beamable stay alive.
			_beamable = FindObjectOfType<BeamableBehaviour>();
			if (_beamable != null)
			{
				SetGameMode(GameMode.AutoHostOrClient);
				ShowUIPanel(_uiCurtain, true);
				ShowUIPanel(_uiStart, false);
			}
        }

        private void Update()
		{
            if (_uiProgress.alpha > 0.0f)
            {
                if (Keyboard.current.escapeKey.wasPressedThisFrame)
                {
					NetworkRunner runner = FindObjectOfType<NetworkRunner>();
					if (runner != null && !runner.IsShutdown)
					{
						Debug.Log("<color=green>GameLauncher Shutdown Triggerd</color>");
						// Calling with destroyGameObject false because we do this in the OnShutdown callback on FusionLauncher
						runner.Shutdown(false);
					}
				}
                UpdateUI();
            }
        }

		// What mode to play - Called from the start menu
		public void OnHostOptions()
		{
			_matchConfig.MatchId = "TestRoom_0";
			SetGameMode(GameMode.Host);
		}

		public void OnJoinOptions()
		{
			_matchConfig.MatchId = "TestRoom_0";
			SetGameMode(GameMode.Client);
		}

		public void OnAutoOptions()
		{
			_matchConfig.MatchId = "AutoRoom";
			SetGameMode(GameMode.AutoHostOrClient);
		}

		private void SetGameMode(GameMode gamemode)
		{
			_gameMode = gamemode;

			// Only show the room input menu if we didn't come from the beamable matchmaking scene.
			if (_matchConfig == null)
			{
				ShowUIPanel(_uiRoom, true);
			}
			else
			{
				OnEnterRoom();
			}

		}

		private void ShowUIPanel(CanvasGroup panel, bool value)
        {
			panel.alpha = (value == true) ? 1.0f : 0.0f;
			panel.interactable = value;
			panel.blocksRaycasts = value;
        }

		public void OnEnterRoom()
		{
			Debug.LogFormat("OnGameModeSelected RoomName - <color=green>{0}</color>", _matchConfig.MatchId);

		    FusionLauncher launcher = FindObjectOfType<FusionLauncher>();
			if (launcher == null)
				launcher = new GameObject("Launcher").AddComponent<FusionLauncher>();

			LevelManager lm = FindObjectOfType<LevelManager>();
			lm.launcher = launcher;

			launcher.Launch(_gameMode, _room.text, lm, _matchConfig, OnConnectionStatusUpdate, OnSpawnWorld, OnSpawnPlayer, OnDespawnPlayer);

            ShowUIPanel(_uiRoom, false);
		}

		private void OnConnectionStatusUpdate(NetworkRunner runner, FusionLauncher.ConnectionStatus status, string reason)
		{
			Debug.LogFormat("OnConnectionStatusUpdate - {0}, {1}", status, reason);

			if (status != _status)
			{
				switch (status)
				{
					case FusionLauncher.ConnectionStatus.Disconnected:
						{
							//ErrorBox.Show("Disconnected!", reason, () => { });
							_loaded = false;
						}
						break;
					case FusionLauncher.ConnectionStatus.Failed:
						{
							ErrorBox.Show("Error!", reason, () => { });
							_loaded = false;
						}
						break;
				}
			}

			_status = status;
			UpdateUI();
		}

		private void OnSpawnWorld(NetworkRunner runner)
		{
			Debug.Log("Spawning GameManager");
			runner.Spawn(_gameManagerPrefab, Vector3.zero, Quaternion.identity, null, InitNetworkState);
			void InitNetworkState(NetworkRunner runner, NetworkObject world)
			{
				world.transform.parent = transform;
			}
		}

		private void OnSpawnPlayer(NetworkRunner runner, PlayerRef playerref)
		{
			Debug.Log("Spawning Player");
			runner.Spawn(_playerPrefab, Vector3.zero, Quaternion.identity, playerref, InitNetworkState);
			void InitNetworkState(NetworkRunner runner, NetworkObject networkObject)
			{
				PlayerCharacter player = networkObject.gameObject.GetComponent<PlayerCharacter>();
				Debug.Log($"Initializing player {player.PlayerRefID}");
				player.InitNetworkState();
			}
			OnPlayerSpawned?.Invoke();
		}

		private void OnDespawnPlayer(NetworkRunner runner, PlayerRef playerref)
		{
			Debug.LogFormat("GameLauncher - OnDespawnPlayer({0})", playerref);
			PlayerCharacter player = PlayerManager.Get(playerref);
			player.TriggerDespawn();
			OnPlayerDespawned?.Invoke();
		}

		private void UpdateUI()
		{
			if (_loaded)
				return;

			bool intro = false;
			bool progress = false;
            bool running = false;

            switch (_status)
			{
				case FusionLauncher.ConnectionStatus.Disconnected:
					{
						_progress.text = "Disconnected!";
						if (_beamable != null)
						{
							intro = false;
							_loaded = false;
						}
						else
						{
							intro = true;
							_loaded = false;
						}
					}
                        break;
				case FusionLauncher.ConnectionStatus.Failed:
					{
						_progress.text = "Failed!";
						intro = true;
						_loaded = false;
					}
					break;
				case FusionLauncher.ConnectionStatus.Connecting:
					{
						_progress.text = "Connecting";
						progress = true;
					}
					break;
				case FusionLauncher.ConnectionStatus.Connected:
					{
						_progress.text = "Connected";
						progress = true;
					}
					break;
				case FusionLauncher.ConnectionStatus.Loading:
					{
						_progress.text = "Loading";
						progress = true;
						_loaded = false;
					}
					break;
				case FusionLauncher.ConnectionStatus.Loaded:
					{
						running = true;
						_loaded = true;
					}
					break;
			}

            ShowUIPanel(_uiCurtain, !running);
            ShowUIPanel(_uiStart, intro);
			ShowUIPanel(_uiProgress, progress);
		}
	}
}
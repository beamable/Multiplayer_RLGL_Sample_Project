using Beamable;
using BeamableExample.FusionHelpers;
using BeamableExample.RedlightGreenLight.Character;
using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BeamableExample.RedlightGreenLight
{
	/// <summary>
	/// The LevelManager controls the map - keeps track of spawn points for players and powerups, and spawns powerups at regular intervals.
	/// </summary>
	public class LevelManager : NetworkSceneManagerBase
	{
		[SerializeField] private int[] _levels;
		[SerializeField] private LevelBehaviour _currentLevel;

		private Scene _loadedScene;

		public RedLightManager redLightManager { get; set; }
		public ReadyupManager readyupManager { get; set; }
		private CountdownManager _countdownManager;
		
		public FusionLauncher launcher { get; set; }

		protected override void Shutdown(NetworkRunner runner)
		{
			base.Shutdown(runner);
			_currentLevel = null;
			if (_loadedScene != default)
			{
				BeamableBehaviour beamable = FindObjectOfType<BeamableBehaviour>();
				if (beamable != null)
				{
					Debug.Log("<color=green>Load Beamable Menu</color>");
					SceneManager.LoadSceneAsync(1);
				}
				else
                {
					Debug.Log("<color=green>Unload Level Scene</color>");
					SceneManager.UnloadSceneAsync(_loadedScene);
                }
			}
			_loadedScene = default;
			PlayerManager.ResetPlayerManager();
		}

		public SpawnPoint GetPlayerSpawnPoint()
		{
			if (_currentLevel != null)
				return _currentLevel.GetPlayerSpawnPoint();
			return null;
		}

		public void LoadLevel(int nextLevelIndex)
		{
			Runner.SetActiveScene(_levels[nextLevelIndex]);
		}

		protected override IEnumerator SwitchScene(SceneRef prevScene, SceneRef newScene, FinishedLoadingDelegate finished)
		{
			Debug.Log($"Switching Scene from {prevScene} to {newScene}");
			if (newScene <= 0)
			{
				finished(new List<NetworkObject>());
				yield break;
			}

			if (Runner.IsServer || Runner.IsSharedModeMasterClient)
				GameManager.playState = GameManager.GameState.TRANSITION;

			// If we are moving from a game scene (i.e. greater than 3 than we need to first 
			// Despawn all the currently spawned players.
			if (prevScene > 2)
			{
				yield return new WaitForSeconds(1.0f);

				// Despawn players with a small delay between each one
				Debug.Log("De-spawning all players");
				for (int i = 0; i < PlayerManager.allPlayers.Count; i++)
				{
					Debug.Log($"De-spawning player {i}:{PlayerManager.allPlayers[i]}");
					PlayerManager.allPlayers[i].DespawnPlayer();
					yield return new WaitForSeconds(0.1f);
				}

				yield return new WaitForSeconds(1.5f - PlayerManager.allPlayers.Count * 0.1f);

				Debug.Log("Despawned all players");
				// Players have despawned
			}

			launcher.SetConnectionStatus(FusionLauncher.ConnectionStatus.Loading, "");

			// Delay one frame
			yield return null;

			Debug.Log($"Start loading scene {newScene} in single peer mode");

			if (_loadedScene != default)
			{
				Debug.Log($"Unloading Scene {_loadedScene.buildIndex}");
				yield return SceneManager.UnloadSceneAsync(_loadedScene);
			}

			_loadedScene = default;
			Debug.Log($"Loading scene {newScene}");

			List<NetworkObject> sceneObjects = new List<NetworkObject>();
			if (newScene >= 2)
			{
				yield return SceneManager.LoadSceneAsync(newScene, LoadSceneMode.Additive);
				yield return 0;
				_loadedScene = SceneManager.GetSceneByBuildIndex(newScene);
				Debug.Log($"Loaded scene {newScene}: {_loadedScene}");
				sceneObjects = FindNetworkObjects(_loadedScene, disable: false);

				SceneManager.SetActiveScene(_loadedScene);
			}

			// Delay one frame
			yield return null;

			launcher.SetConnectionStatus(FusionLauncher.ConnectionStatus.Loaded, "");

			// Activate the next level
			_currentLevel = FindObjectOfType<LevelBehaviour>();
			if (_currentLevel != null)
				_currentLevel.Initialize(Runner);

			Debug.Log($"Switched Scene from {prevScene} to {newScene} - loaded {sceneObjects.Count} scene objects");
			finished(sceneObjects);

			// Show the ready up UI if we loaded into a game scene.
			if (newScene >= 3)
			{
				readyupManager = FindObjectOfType<ReadyupManager>(true);

				//if (readyupManager != null)
				//	readyupManager.ShowUI();

				// TODO:: We need to make this into a base class so we can have other game levels/managers.
				redLightManager = FindObjectOfType<RedLightManager>(true);
			}
			else
			{
				readyupManager = FindObjectOfType<ReadyupManager>(true);

				if (readyupManager != null)
					readyupManager.HideReadyUpUI();
			}

			yield return new WaitForSeconds(0.3f);

			// Respawn with slight delay between each player
			Debug.Log($"Respawning All Players");
			for (int i = 0; i < PlayerManager.allPlayers.Count; i++)
			{
				PlayerCharacter player = PlayerManager.allPlayers[i];
				Debug.Log($"Respawning Player {i}:{player}");
				player.Respawn(0);
				yield return new WaitForSeconds(0.3f);
			}

			if (_loadedScene.buildIndex >= 3)
			{
				// We are in game.
				if (Runner != null && (Runner.IsServer || Runner.IsSharedModeMasterClient))
				{
					GameManager.playState = GameManager.GameState.LOBBY;
				}

				Debug.Log($"Switched Scene from {prevScene} to {newScene}");
			}
		}

		public void StartLevel()
		{
			Debug.Log("StartLevel");

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
					// TODO:: We need to make this into a base class so we can have other game levels/managers.
					redLightManager.Reset();
					redLightManager.BeginGame();
				}));
			}));
		}
	}
}
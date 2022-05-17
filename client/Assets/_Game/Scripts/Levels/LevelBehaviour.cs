using System;
using System.Linq;
using _Game.Scripts.Levels;
using UnityEngine;
using Fusion;

namespace BeamableExample.RedlightGreenLight
{
	public class LevelBehaviour : MonoBehaviour
	{
		private NetworkedLevelObject[] _networkedObjects;
		private SpawnPointGroup[] spawnGroups;
		private int currentSpawnGroupIndex = 0;
		private NetworkRunner _runner;
		private NetworkObject _levelSoundManager;

		[SerializeField] private NetworkObject LevelSoundManagerPrefab;

		private void Start()
		{
			spawnGroups = GetComponentsInChildren<SpawnPointGroup>(true);
		}

		public void Initialize(NetworkRunner runner)
		{
			runner.Spawn(LevelSoundManagerPrefab, transform.position, transform.rotation);
		}

		// A variation that also spawns any networkobjects present
		public void ActivateSpawners(NetworkRunner runner)
		{
			_runner = runner;

			if (GameManager.Instance.Object.HasStateAuthority)
			{
				_networkedObjects = gameObject.GetComponentsInChildren<NetworkedLevelObject>();
				for (int i = 0; i < _networkedObjects.Length; i++)
				{
					_networkedObjects[i].SpawnPrefab(runner);
				}
			}
		}

		// Despawn networkobjects if any, and set level inactive
		public void Deactivate()
		{
			if (_networkedObjects != null && _networkedObjects.Length > 0)
			{
				for (int i = 0; i < _networkedObjects.Length; i++)
				{
					_networkedObjects[i].DespawnPrefab(_runner);
				}
				_networkedObjects = null;
			}
		}

		public SpawnPoint GetPlayerSpawnPoint()
		{
			var validSpawnPoints = spawnGroups[currentSpawnGroupIndex].spawnPoints.Where(point => !point.occupied).ToArray();
			var targetSpawnPoint = validSpawnPoints[validSpawnPoints.Length - 1];
			targetSpawnPoint.occupied = true;
			if (validSpawnPoints.Length == 1)
			{
				IncrementSpawnGroup();	
			}
			return targetSpawnPoint;
		}

		private void IncrementSpawnGroup()
		{
			currentSpawnGroupIndex++;
			if (currentSpawnGroupIndex == spawnGroups.Length)
			{
				currentSpawnGroupIndex = 0;
			}
		}

		private void TestSpawnPoints()
		{
			for (var i = 0; i < 200; i++)
			{
				GetPlayerSpawnPoint();
			}

			foreach (var group in spawnGroups)
			{
				var occupiedSpawnPoints = group.spawnPoints.Where(point => point.occupied).ToArray();
				if (occupiedSpawnPoints.Length != 25)
				{
					Debug.LogError($"Uneven slice count detected! Slice count: {occupiedSpawnPoints.Length} on slice {group.name}");
				}
			}
		}
	}
}
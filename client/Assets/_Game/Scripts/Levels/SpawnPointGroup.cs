using BeamableExample.RedlightGreenLight;
using UnityEngine;

namespace _Game.Scripts.Levels
{
    public class SpawnPointGroup : MonoBehaviour
    {
        public SpawnPoint[] spawnPoints { get; private set; }

        void Awake()
        {
            spawnPoints = GetComponentsInChildren<SpawnPoint>(true);
        }
    }
}

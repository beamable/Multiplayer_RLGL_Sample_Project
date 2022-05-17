using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

public class NetworkGameHandler : MonoBehaviour
{
    [SerializeField] private NetworkRunner runner;
    [SerializeField] private MatchConfig currentMatch;
    
    private void Start()
    {
        var sceneObjectProvider = GetSceneObjectProvider();
        
        var sceneRef = 1; 
        var args = new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            Address = NetAddress.Any(),
            Scene = sceneRef,
            SessionName = currentMatch.MatchId,
            Initialized = null,
            SceneObjectProvider = sceneObjectProvider
            
        };
        runner.StartGame(args);
    }

    private INetworkSceneObjectProvider GetSceneObjectProvider()
    {
        var sceneObjectProvider = runner.GetComponents(typeof(MonoBehaviour)).OfType<INetworkSceneObjectProvider>().FirstOrDefault();
        if (sceneObjectProvider != null) return sceneObjectProvider;
        
        Debug.Log($"NetworkRunner does not have any component implementing {nameof(INetworkSceneObjectProvider)} interface, adding {nameof(NetworkSceneManagerDefault)}.", runner);
        sceneObjectProvider = runner.gameObject.AddComponent<NetworkSceneManagerDefault>();
        return sceneObjectProvider;
    }
}
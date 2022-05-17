using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace _Game.Features.Loading
{
    public class SceneLoader : MonoBehaviour
    {
        [SerializeField] private int sceneIndex;
        [SerializeField] private string sceneName;

        [SerializeField] private UnityEvent<float> OnProgressUpdated;

        private bool _isLoading;
        private AsyncOperation loadingOperation;
    
        public void LoadSceneByIndex()
        {
            if (_isLoading) return;
            loadingOperation = SceneManager.LoadSceneAsync(sceneIndex);
            _isLoading = true;
        }

        public void LoadSceneByName()
        {
            if (_isLoading) return;
            loadingOperation = SceneManager.LoadSceneAsync(sceneName);
            _isLoading = true;
        }

        private void Update()
        {
            if (_isLoading)
            {
                OnProgressUpdated?.Invoke(loadingOperation.progress);
            }
        }
    }
}

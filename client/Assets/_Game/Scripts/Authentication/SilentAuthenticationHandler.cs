using Beamable;
using UnityEngine;
using UnityEngine.Events;

namespace _Game.Features.Authentication
{
    public class SilentAuthenticationHandler : MonoBehaviour
    {
        [SerializeField]
        private UnityEvent OnLoginSuccess;
    
        private IBeamableAPI _beamableAPI;
        
        private async void Awake()
        {
            _beamableAPI = await API.Instance;
            Debug.Log($"User Id: {_beamableAPI.User.id}");
            OnLoginSuccess?.Invoke();
        }
    }
}
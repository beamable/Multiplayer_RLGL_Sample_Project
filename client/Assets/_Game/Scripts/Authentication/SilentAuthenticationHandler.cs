using Beamable;
using UnityEngine;
using UnityEngine.Events;

namespace _Game.Features.Authentication
{
    public class SilentAuthenticationHandler : MonoBehaviour
    {
        [SerializeField]
        private UnityEvent OnLoginSuccess;
        private BeamContext _context;
        
        private async void Awake()
        {
            _context = BeamContext.Default;
            await _context.OnReady;
            Debug.Log($"User Id: {_context.PlayerId}");
            OnLoginSuccess?.Invoke();
        }
    }
}
using _Game.Features.Authentication;
using Beamable;
using UnityEngine;

public class GuestLoginHandler : AuthenticationHandler
{
    protected override async void Awake()
    {
        _context = BeamContext.Default;
        await _context.OnReady;
        Debug.Log($"User Id: {(_context.PlayerId)}");
    }
    
    public override void Login()
    {
        _context.ClearPlayerAndStop();
        _context.Start();
        SetDefaultAlias();
        OnLoginSuccess?.Invoke();
    }
}

using System;
using _Game.Features.Authentication;
using Beamable;
using Beamable.Common.Api.Auth;
using JetBrains.Annotations;
using UnityEngine;

public class EmailLoginHandler : AuthenticationHandler
{
    private User _user;

    [HideInInspector] public string email;
    [HideInInspector] public string password;

    protected override async void Awake()
    {
        base.Awake();
        _context = BeamContext.Default;
        await _context.OnReady;
    }

    [UsedImplicitly]
    public void SetEmail(string newEmail)
    {
        email = newEmail;
    }

    [UsedImplicitly]
    public void SetPassword(string newPassword)
    {
        password = newPassword;
    }

    [UsedImplicitly]
    public async void CreateUser()
    {
        var token = await _context.Api.AuthService.CreateUser();
        _context.Api.ApplyToken(token);
        _context.Api.AuthService.RegisterDBCredentials(email, password)
            .Then(user =>
            {
                _user = user;
                Login();
            })
            .Error(error =>
            {
                Debug.LogException(error);
                OnLoginFailed?.Invoke();
            });
    }

    [UsedImplicitly]
    public override async void Login()
    {
        try
        {
            var token = await _context.Api.AuthService.Login(email, password, false);
            _user = _context.Api.User;
            _context.Api.ApplyToken(token);
            OnLoginSuccess?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            OnLoginFailed?.Invoke();
        }    
    }
}

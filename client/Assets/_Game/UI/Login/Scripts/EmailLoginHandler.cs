using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using _Game.Features.Authentication;
using Beamable;
using Beamable.Common.Api.Auth;
using JetBrains.Annotations;
using UnityEngine;

public class EmailLoginHandler : AuthenticationHandler
{
    private IBeamableAPI _beamableAPI;
    private User _user;

    [HideInInspector] public string email;
    [HideInInspector] public string password;

    protected override async void Awake()
    {
        base.Awake();
        _beamableAPI = await API.Instance;
        
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
        var token = await _beamableAPI.AuthService.CreateUser();
        _beamableAPI.ApplyToken(token);
        _beamableAPI.AuthService.RegisterDBCredentials(email, password)
            .Then(user =>
            {
                _user = user;
                OnLoginSuccess?.Invoke();
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
            var token = await _beamableAPI.AuthService.Login(email, password, false);
            _user = _beamableAPI.User;
            _beamableAPI.ApplyToken(token);
            OnLoginSuccess?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            OnLoginFailed?.Invoke();
        }    
    }
}

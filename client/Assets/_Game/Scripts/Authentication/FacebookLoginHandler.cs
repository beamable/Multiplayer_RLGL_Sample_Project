using System.Collections.Generic;
using Beamable.Common.Api.Auth;
using Facebook.Unity;
using JetBrains.Annotations;
using UnityEngine;

namespace _Game.Features.Authentication
{
    public class FacebookLoginHandler : AuthenticationHandler
    {
        protected override void Awake()
        {
            base.Awake();
            FB.Init();
        }

        [UsedImplicitly]
        public override void Login()
        {
            var perms = new List<string>{"public_profile", "email"};
            FB.LogInWithReadPermissions(perms, FBAuthCallback);
        }

        private async void FBAuthCallback(ILoginResult result)
        {
            if (result.Cancelled)
            {
                Debug.Log("Facebook login result: cancelled");
                OnLoginCancelled?.Invoke();
                return;
            }

            if (result.Error != null)
            {
                Debug.Log("Facebook login result: error");
                Debug.LogError(result.Error);
                OnLoginFailed?.Invoke();
                return;
            }
            
            await RegisterThirdPartyCredentials(AuthThirdParty.Facebook, result.AccessToken.TokenString);
            Debug.Log("Facebook login result: success");
            SetDefaultAlias();
            OnLoginSuccess?.Invoke();
        }
    }
}
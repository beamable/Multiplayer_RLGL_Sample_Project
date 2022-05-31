using Beamable.AccountManagement;
using Beamable.Common.Api.Auth;
using Beamable.Platform.SDK.Auth;
using Beamable.Serialization.SmallerJSON;
using JetBrains.Annotations;
using UnityEngine;

namespace _Game.Features.Authentication
{
    public class GoogleLoginHandler : AuthenticationHandler
    {
        private GoogleSignIn _googleSignIn;

        public override void Login()
        {
            var webClientId = AccountManagementConfiguration.Instance.GoogleClientID;
            _googleSignIn = new GoogleSignIn(gameObject, "GoogleAuthResponse", webClientId, null);
            Debug.Log("Google Sign-In Web Client ID: " + webClientId);
            _googleSignIn.Login();
            Debug.Log("Google Login ran!");
        }

        [UsedImplicitly]
        public void GoogleAuthResponse(string message)
        {
            Debug.Log("Handling a Google sign in response!" + message);
            GoogleSignIn.HandleResponse(message, async token =>
            {
                if (token != null)
                {
                    Debug.Log("Valid token handled!");
                    await RegisterThirdPartyCredentials(AuthThirdParty.Google, token);
                    SetDefaultAlias();
                    OnLoginSuccess?.Invoke();
                }
                else
                {
                    Debug.Log("Login was cancelled!");
                    OnLoginCancelled?.Invoke();
                }
            }, errback =>
            {
                Debug.Log("Login had an error!");
                Debug.LogError(errback);
                OnLoginFailed?.Invoke();
            });
        }
    }
}
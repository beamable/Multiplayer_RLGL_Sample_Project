#if !(UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX)
#define DISABLESTEAMWORKS
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Beamable.Common;
using Beamable.Common.Api.Auth;
using Beamable.Common.Steam;
using UnityEngine;

#if !DISABLESTEAMWORKS
using Steamworks;
using UnityEngine.UI;
#endif

namespace _Game.Features.Authentication
{
    public class SteamLoginHandler : AuthenticationHandler
    {
#if !DISABLESTEAMWORKS
        private string _ticket;
        [SerializeField] private Button signInButton;
        
        protected override async void Awake()
        {
            base.Awake();
            if (!SteamManager.Initialized)
            {
                signInButton.interactable = false;
                return;
            }
            var steamUserID = SteamUser.GetSteamID().ToString();
            Debug.Log($"SteamUserID:{steamUserID}");
    
            _ticket = await GetSteamAuthTicket();
        }

        public override async void Login()
        {
            if (_ticket == null)
            {
                OnLoginFailed?.Invoke();
                return;
            }

            SetAvailability(await BeamableValidateSteamTicket(_ticket));
            await RegisterThirdPartyCredentials(AuthThirdParty.Steam, GetTokenFromTicket(_ticket));
            OnLoginSuccess?.Invoke();
        }

        private string GetTokenFromTicket(string ticket)
        {
            var request = new AuthenticateUserRequest(SteamUser.GetSteamID().ToString(), ticket);
            var steamRequest = JsonUtility.ToJson(request);
            var encodedRequest = Encoding.UTF8.GetBytes(steamRequest);
            var token = Convert.ToBase64String(encodedRequest);

            return token;
        }
        
        /// <summary>
        /// Get the Auth ticket for the current Steam User
        /// </summary>
        /// <param name="hAuthTicket"></param>
        /// <returns>an string auth ticket provided from steam</returns>
        private Promise<string> GetSteamAuthTicket()
        {
            var promise = new Promise<string>();
            var steamAuthTicketBuffer = new byte[1024];
            uint steamAuthTicketBufferSize = 1024;

            Callback<GetAuthSessionTicketResponse_t>.Create(_ =>
            {
                var usedBytes = new List<byte>(steamAuthTicketBuffer).GetRange(0, (int) steamAuthTicketBufferSize).ToArray();
                var ticket = BitConverter.ToString(usedBytes).Replace("-", string.Empty);
                promise.CompleteSuccess(ticket);
            });

            SteamUser.GetAuthSessionTicket(steamAuthTicketBuffer, (int) steamAuthTicketBufferSize, out steamAuthTicketBufferSize);

            return promise;
        }
        
        private Promise<bool> BeamableValidateSteamTicket(string ticket)
        {
            var promise = new Promise<bool>();
    
            _beamableAPI.Requester.Request<Beamable.Common.Api.EmptyResponse>(
                    Beamable.Common.Api.Method.POST,
                    $"/basic/payments/steam/auth",
                    new SteamTicketRequest(ticket))
                .Then(f =>
                {
                    promise.CompleteSuccess(true);
                })
                .Error(ex=>{
                    Debug.LogError(ex);
                    promise.CompleteSuccess(false);
                });
    
            return promise;
        }
#else
        public override void Login() {}
#endif
    }
}

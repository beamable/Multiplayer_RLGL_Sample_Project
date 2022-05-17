using System.Threading.Tasks;
using Beamable;
using Beamable.Common.Api.Auth;
using Beamable.Server.Clients;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;

namespace _Game.Features.Authentication
{
    public abstract class AuthenticationHandler : MonoBehaviour
    {
        [SerializeField]
        protected UnityEvent OnLoginSuccess;
        [SerializeField]
        protected UnityEvent OnLoginCancelled;
        [SerializeField]
        protected UnityEvent OnLoginFailed;

        [SerializeField] private BeamableStatsController statsController;
    
        protected IBeamableAPI _beamableAPI;

        protected bool _isAvailable;

        public abstract void Login();

        [UsedImplicitly]
        public async void GiveNotABotAchievement()
        {
            await statsController.ChangeStat("CREATE_AN_ACCOUNT", "True");
        }

        protected virtual async void Awake()
        {
            _beamableAPI = await API.Instance;
            Debug.Log($"User Id: {_beamableAPI.User.id}");

            await CheckAutoLogin();
        }
        private async Task CheckAutoLogin()
        {
            if (!_beamableAPI.User.HasAnyCredentials()) return;
            await LoginWithToken();
        }
        protected async Task RegisterThirdPartyCredentials(AuthThirdParty thirdParty, string token)
        {
            if (thirdParty != AuthThirdParty.Steam)
            {
                SetAvailability(await _beamableAPI.AuthService.IsThirdPartyAvailable(thirdParty, token));
            }
            var userHasCredentials = _beamableAPI.User.HasThirdPartyAssociation(thirdParty);

            var shouldSwitchUsers = !_isAvailable;
            var shouldCreateUser = _isAvailable && userHasCredentials;
            var shouldAttachToCurrentUser = _isAvailable && !userHasCredentials;
            
            if(shouldSwitchUsers)
            {
                await SwitchUsers(thirdParty, token);
            }
            
            if(shouldCreateUser)
            {
                await CreateUser(thirdParty, token);
            }
            
            if(shouldAttachToCurrentUser)
            {
                await LinkToExistingUser(thirdParty, token);
            }
        }

        protected void SetAvailability(bool available)
        {
            _isAvailable = available;
        }

        private async Task SwitchUsers(AuthThirdParty thirdParty, string token)
        {
            await _beamableAPI.AuthService.LoginThirdParty(thirdParty, token, false);
        }

        private async Task CreateUser(AuthThirdParty thirdParty, string token)
        {
            var tokenResponse = await _beamableAPI.AuthService.CreateUser();
            _beamableAPI.ApplyToken(tokenResponse);
            var user = await _beamableAPI.AuthService.RegisterThirdPartyCredentials(thirdParty, token);
            _beamableAPI.UpdateUserData(user);
            GiveNotABotAchievement();
        }

        private async Task LinkToExistingUser(AuthThirdParty thirdParty, string token)
        {
            var user = await _beamableAPI.AuthService.RegisterThirdPartyCredentials(thirdParty, token);
            _beamableAPI.UpdateUserData(user);
        }
        
        private async Task LoginWithToken()
        {
            OnLoginSuccess?.Invoke();
        }
    }
}
using System.Threading.Tasks;
using _Game.UI.AccountSelection.Scripts;
using Beamable;
using Beamable.Common.Api.Auth;
using UnityEngine;
using UnityEngine.Events;

namespace _Game.Features.Authentication
{
    public abstract class AuthenticationHandler : MonoBehaviour
    {
        private const string ALIAS_KEY = "alias";
        [SerializeField] 
        protected string defaultAlias = "Anonymous";
        [SerializeField]
        protected UnityEvent OnLoginSuccess;
        [SerializeField]
        protected UnityEvent OnLoginCancelled;
        [SerializeField]
        protected UnityEvent OnLoginFailed;

        protected BeamContext _context;

        protected bool _isAvailable;

        [SerializeField] 
        private AccountSelectionController _accountSelectionController;

        public abstract void Login();

        protected virtual async void Awake()
        {
            _context = BeamContext.Default;
            await _context.OnReady;
            Debug.Log($"User Id: {(_context.PlayerId)}");

            await CheckAutoLogin();
        }
        private async Task CheckAutoLogin()
        {
            var otherAccounts = await _accountSelectionController.GetOtherAccounts();
            if (otherAccounts.Count > 0 || _context.Api.User.HasAnyCredentials())
            {
                await LoginWithToken();
            }
        }
        protected async Task RegisterThirdPartyCredentials(AuthThirdParty thirdParty, string token)
        {
            if (thirdParty != AuthThirdParty.Steam)
            {
                SetAvailability(await _context.Api.AuthService.IsThirdPartyAvailable(thirdParty, token));
            }
            var userHasCredentials = _context.Api.User.HasThirdPartyAssociation(thirdParty);

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
            await _context.Api.AuthService.LoginThirdParty(thirdParty, token, false);
        }

        private async Task CreateUser(AuthThirdParty thirdParty, string token)
        {
            var tokenResponse = await _context.Api.AuthService.CreateUser();
            await _context.Api.ApplyToken(tokenResponse);
            var user = await _context.Api.AuthService.RegisterThirdPartyCredentials(thirdParty, token);
            _context.Api.UpdateUserData(user);
            SetDefaultAlias();
        }

        private async Task LinkToExistingUser(AuthThirdParty thirdParty, string token)
        {
            var user = await _context.Api.AuthService.RegisterThirdPartyCredentials(thirdParty, token);
            _context.Api.UpdateUserData(user);
            SetDefaultAlias();
        }
        
        private Task LoginWithToken()
        {
            SetDefaultAlias();
            OnLoginSuccess?.Invoke();
            return Task.CompletedTask;
        }

        protected async void SetDefaultAlias()
        {
            if (!await BeamableStatsController.CheckIfHasStat(ALIAS_KEY))
            {
                await BeamableStatsController.ChangeStat(ALIAS_KEY, defaultAlias);
            }
        }
    }
}
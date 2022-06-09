using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beamable;
using Beamable.Common.Api.Auth;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;

namespace _Game.UI.AccountSelection.Scripts
{
    public class AccountSelectionController : MonoBehaviour
    {
        [HideInInspector]
        public BeamContext context;
        public UnityEvent OnSwitchUser;
        public UnityEvent<string> OnSwitchWithEmail;
        public UnityEvent OnSwitchToSignIn;
        [HideInInspector]
        public UserBundle selectedUser;

        private async void Awake()
        {
            await SetupBeamable();
        }

        private async Task SetupBeamable()
        {
            context = BeamContext.Default;
            await context.OnReady;
        }

        /// <summary>
        /// Returns the account the user is currently logged in to.
        /// </summary>
        public async Task<BeamContext> GetCurrentAccount()
        {
            await SetupBeamable();
            return context;
        }

        public async Task<UserBundle> GetCurrentAccountBundle()
        {
            await SetupBeamable();
            var accounts = await GetAllAccounts();
            foreach (var account in accounts)
            {
                if (account.User.id == context.Api.User.id)
                {
                    return account;
                }
            }

            return new UserBundle();
        }

        /// <summary>
        /// Returns accounts on this device that the user is not logged in to.
        /// </summary>
        public async Task<List<UserBundle>> GetOtherAccounts()
        {
            await SetupBeamable();
            var accounts = (await GetAllAccounts()).ToList();
            var currentAccount = await GetCurrentAccount();
            accounts.Remove(accounts.Find(userBundle => userBundle.User.id == currentAccount.Api.User.id));
            return accounts;
        }

        /// <summary>
        /// Returns all accounts that have logged in on this device.
        /// </summary>
        public async Task<IEnumerable<UserBundle>> GetAllAccounts()
        {
            await SetupBeamable();
            return await context.Api.GetDeviceUsers();
        }

        /// <summary>
        /// Returns a list of third party auth types that the specified user has associated with their account.
        /// </summary>
        /// <param name="context">The context of the user to be checked.</param>
        public static List<AuthThirdParty> GetThirdPartyAssociations(BeamContext context)
        {
            var thirdPartyTypes = Enum.GetValues(typeof(AuthThirdParty));
            var possibleTypes = thirdPartyTypes.Cast<AuthThirdParty>();
            var validTypes = possibleTypes.Where(context.Api.User.HasThirdPartyAssociation);
            return validTypes.ToList();
        }

        [UsedImplicitly]
        public async void SwitchCurrentUser()
        {
            if (selectedUser.User == context.Api.User)
            {
                OnSwitchToSignIn?.Invoke();
                return;
            }

            if (selectedUser.User != null)
            {
                context = await context.ChangeAuthorizedPlayer(selectedUser.Token).Error(exception =>
                {
                    if (selectedUser.User.HasDBCredentials())
                    {
                        OnSwitchWithEmail?.Invoke(selectedUser.User.email);
                    }
                    else
                    {
                        OnSwitchToSignIn?.Invoke();
                    }
                });
            }

            await SetupBeamable();
            OnSwitchUser?.Invoke();
        }
    }
}

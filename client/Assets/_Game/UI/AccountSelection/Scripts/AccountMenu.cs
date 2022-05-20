using System.Collections.Generic;
using System.Threading.Tasks;
using Beamable;
using Beamable.Common.Api.Auth;
using Beamable.Stats;
using ListView;
using UnityEngine;

namespace _Game.UI.AccountSelection.Scripts
{
    /// <summary>
    /// A base class for UI that displays one or more user accounts.
    /// </summary>
    public class AccountMenu : MonoBehaviour
    {
        [SerializeField]
        protected AccountSelectionController controller;

        [SerializeField] private StatObject scoreStat;
        [SerializeField] private StatObject aliasStat;
        [SerializeField] private StatObject avatarStat;

        private string _alias;
        private string _score;
        private string _avatar;

        /// <summary>
        /// Generates list item data based on the passed in user.
        /// </summary>
        protected async Task<ListItem> GenerateAccountItem(UserBundle userBundle)
        {
            var user = userBundle.User;
            await GetAccountStats(user);
            return new ListItem
            {
                Id = user.id.ToString(),
                Title = _alias,
                PropertyBag = new Dictionary<string, object>()
                {
                    {"thirdPartyAppAssociations", user.thirdPartyAppAssociations},
                    {"email", user.email},
                    {"score", _score},
                    {"avatar", _avatar}
                },
                ViewAction = () =>
                {
                    controller.selectedUser = userBundle;
                }
            };
        }

        private async Task GetAccountStats(User user)
        {
            var context = BeamContext.Default;
            await context.OnReady;
            var stats = await context.Api.StatsService.GetStats("client", "public", "player", user.id);
            
            stats.TryGetValue(aliasStat.StatKey, out _alias);
            stats.TryGetValue(scoreStat.StatKey, out _score);
            stats.TryGetValue(avatarStat.StatKey, out _avatar);
        }
    }
}

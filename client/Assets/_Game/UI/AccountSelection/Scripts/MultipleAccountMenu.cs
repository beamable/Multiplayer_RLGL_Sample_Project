using System.Collections.Generic;
using Beamable;
using Beamable.Common.Api.Auth;
using ListView;
using UnityEngine;

namespace _Game.UI.AccountSelection.Scripts
{
    public class MultipleAccountMenu : AccountMenu
    {
        [SerializeField]
        private ListViewComponent accountsListView;

        private void OnEnable()
        {
            CreateMultipleMenu();
        }
        
        private async void BuildOtherAccountsList(List<UserBundle> users)
        {
            var cardData = new ListViewData();
            foreach (var user in users)
            {
                cardData.Add(await GenerateAccountItem(user));
            }
            accountsListView.Build(cardData);
        }

        public async void CreateMultipleMenu()
        {
            var otherAccounts = await controller.GetOtherAccounts();
            var otherUsers = new List<UserBundle>();
            foreach (var bundle in otherAccounts)
            {
                otherUsers.Add(bundle);
            }
            BuildOtherAccountsList(otherUsers);
        }
    }
}

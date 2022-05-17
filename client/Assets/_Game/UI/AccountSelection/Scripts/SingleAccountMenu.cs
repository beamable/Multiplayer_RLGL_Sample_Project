using ListView;
using UnityEngine;

namespace _Game.UI.AccountSelection.Scripts
{
    public class SingleAccountMenu : AccountMenu
    {
        [SerializeField]
        private ListCard userCard;

        private void OnEnable()
        {
            CreateSingleMenu();
        }

        public async void CreateSingleMenu()
        {
            var currentUser = await controller.GetCurrentAccountBundle();
            var listItem = await GenerateAccountItem(currentUser);
            userCard.SetUp(listItem);
        }
    }
}
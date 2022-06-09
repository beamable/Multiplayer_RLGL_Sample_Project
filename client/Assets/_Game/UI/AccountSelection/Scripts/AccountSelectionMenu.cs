using _Game.UI.AccountSelection.Scripts;
using Beamable.UI;
using UnityEngine;

namespace _Game.UI.Login.Scripts
{
    public class AccountSelectionMenu : MonoBehaviour
    {
        [SerializeField]
        private AccountSelectionController controller;
        [SerializeField]
        private UiViewComponent singleAccountView;
        [SerializeField]
        private UiViewComponent multipleAccountView;

        private async void OnEnable()
        {
            var otherUsers = await controller.GetOtherAccounts();
            if (otherUsers.Count == 0)
            {
                SwitchToSingleAccountView();
                return;
            }
            SwitchToMultipleAccountView();
        }

        private void SwitchToSingleAccountView()
        {
            multipleAccountView.Hide();
            singleAccountView.Show();
        }
        
        private void SwitchToMultipleAccountView()
        {
            singleAccountView.Hide();
            multipleAccountView.Show();
        }
    }
}
using Beamable.UI;
using BeamableExample.RedlightGreenLight;
using BeamableExample.RedlightGreenLight.Character;
using TMPro;
using UnityEngine;

namespace BeamableExample.RedlightGreenLight
{
    public class SpectatorUI : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;

        public System.Action<int> ChangeSpectatorCallback;

        public void Initialize(bool show)
        {
            Show(show);
        }

        public void Show(bool value)
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
            
            if (value)
            {
                UiStateController.Show("spectator","game");
                UiStateController.Hide("hud", "game");
            }
            else
            {
                UiStateController.Hide("spectator","game");
            }
        }

        public void OnPrevSpectatorSelected()
        {
            ChangeSpectatorCallback?.Invoke(-1);
        }

        public void OnNextSpectatorSelected()
        {
            ChangeSpectatorCallback?.Invoke(1);
        }

        public void OnQuitSelected()
        {
            UiStateController.Show("loading", "game");
            // End the game and go back to the main menu.
            GameManager.Instance.Restart(Fusion.ShutdownReason.Ok);
        }
    }
}

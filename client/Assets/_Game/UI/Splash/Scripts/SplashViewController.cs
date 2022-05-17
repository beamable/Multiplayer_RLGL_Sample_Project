using Beamable.UI;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Game.UI.Splash.Scripts
{
    public class SplashViewController : MonoBehaviour
    {
        [SerializeField]
        private UiViewComponent splashView;

        private void Update()
        {
            if (Keyboard.current.anyKey.wasPressedThisFrame)
            {
                HideSplashView();
            }
        }

        [UsedImplicitly]
        public void HideSplashView()
        {
            splashView.Hide();
        }
    }
}
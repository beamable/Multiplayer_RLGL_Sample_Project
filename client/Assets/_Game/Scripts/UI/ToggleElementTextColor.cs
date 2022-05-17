using JetBrains.Annotations;
using TMPro;
using UnityEngine;

namespace _Game.Scripts.UI
{
    public class ToggleElementTextColor : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI targetText;
        [SerializeField]
        private Color enabledColor;
        [SerializeField]
        private Color disabledColor;

        [UsedImplicitly]
        public void Enable()
        {
            targetText.color = enabledColor;
        }

        [UsedImplicitly]
        public void Disable()
        {
            targetText.color = disabledColor;
        }
    }
}

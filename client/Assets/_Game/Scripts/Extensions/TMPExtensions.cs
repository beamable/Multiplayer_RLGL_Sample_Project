using TMPro;
using UnityEngine;

namespace _Game.Extensions
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class TMPExtensions : MonoBehaviour
    {
        private TextMeshProUGUI tmp;

        public Color textColor;

        private void Awake()
        {
            tmp = GetComponent<TextMeshProUGUI>();
        }

        public void SetColor(int index)
        {
            //tmp.color = color;
        }
    }
}
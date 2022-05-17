using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace _Game.Scripts.UI
{
    [RequireComponent(typeof(Toggle))]
    public class ToggleEventHandler : MonoBehaviour
    {
        [SerializeField]
        private UnityEvent onToggleEnabled;
        [SerializeField]
        private UnityEvent onToggleDisabled;
        
        private Toggle toggle;

        private void Awake()
        {
            toggle = GetComponent<Toggle>();
            toggle.onValueChanged.AddListener(ValueChanged);
        }

        private void ValueChanged(bool isOn)
        {
            if (isOn)
            {
                onToggleEnabled?.Invoke();
                return;
            }
            onToggleDisabled?.Invoke();
        }
    }
}
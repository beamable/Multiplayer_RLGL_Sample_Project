using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace _Game.Scripts.MobileInput
{
    /// <summary>
    /// A subclass of UnityEngine.UI.Button that triggers an OnClick when
    /// the pointer is down, rather than when the pointer is released.
    /// </summary>
    public class MobileInputButton : Button
    {   
        public override void OnPointerUp(PointerEventData eventData) { }
        public override void OnPointerClick(PointerEventData eventData) { }
        public override void OnPointerDown(PointerEventData eventData)
        {
            OnPointerClick(eventData);
            onClick?.Invoke();
        }
    }
}

using BeamableExample.RedlightGreenLight;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _Game.Scripts.MobileInput
{
    public class MobilePickupButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public void OnPointerDown(PointerEventData eventData)
        {
            if (PlayerInputListener.Instance == null) return;
            PlayerInputListener.Instance.Action = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (PlayerInputListener.Instance == null) return;
            PlayerInputListener.Instance.Action = false;
        }
    }
}

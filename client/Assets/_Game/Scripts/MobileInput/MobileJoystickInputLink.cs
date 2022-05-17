using BeamableExample.RedlightGreenLight;
using UnityEngine;

namespace _Game.Scripts.MobileInput
{
    public class MobileJoystickInputLink : MonoBehaviour
    {
        public void SendMoveInput(Vector2 direction)
        {
            if (PlayerInputListener.Instance == null) return;
            PlayerInputListener.Instance.MoveInputVector = direction;
        }
        
        public void SendLookInput(Vector2 direction)
        {
            if (PlayerInputListener.Instance == null) return;
            PlayerInputListener.Instance.LookInputVector = direction;
        }
    }
}
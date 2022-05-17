using BeamableExample.RedlightGreenLight;
using UnityEngine;

namespace _Game.Scripts.MobileInput
{
    public class MobileButtonInputLink : MonoBehaviour
    {
        public void SendAttackInput()
        {
            if (PlayerInputListener.Instance == null) return;
            PlayerInputListener.Instance.Attack = true;
        }
        
        public void SendJumpInput()
        {
            if (PlayerInputListener.Instance == null) return;
            PlayerInputListener.Instance.Jump = true;
        }

        public void SendSprintInput()
        {
            if (PlayerInputListener.Instance == null) return;
            PlayerInputListener.Instance.Sprint = true;
        }
    }
}
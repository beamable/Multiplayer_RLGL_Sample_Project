using BeamableExample.RedlightGreenLight.Character;
using UnityEngine;

namespace BeamableExample.RedlightGreenLight
{
    public class UICanvasControllerInput : MonoBehaviour
    {
        [Header("Output")]
        public PlayerInput playerInput;
        public CinemachineManager playerCamera;

        public void VirtualMoveInput(Vector2 virtualMoveDirection)
        {
            PlayerInputListener.Instance.OnMoveTouch(virtualMoveDirection);
        }

        public void VirtualLookInput(Vector2 virtualLookDirection)
        {
            PlayerInputListener.Instance.OnLookTouch(virtualLookDirection);
        }

        public void VirtualJumpInput(bool virtualJumpState)
        {
            PlayerInputListener.Instance.OnJumpTouch(virtualJumpState);
        }

        public void VirtualSprintInput(bool virtualSprintState)
        {
            PlayerInputListener.Instance.OnSprintTouch(virtualSprintState);
        }
        
    }

}

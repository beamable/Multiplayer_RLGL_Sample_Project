using BeamableExample.Helpers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BeamableExample.RedlightGreenLight
{
    public class PlayerInputListener : SingletonNonPersistant<PlayerInputListener>
    {
		private Vector2 _moveInputVector;
		private Vector2 _lookInputVector;
		private bool _attack = false;
		private bool _jump = false;
		private bool _sprint = false;
		private bool _ready = false;
		private bool _action = false;
		public string currentControlScheme = "";

		public PlayerInput playerInput;

		public System.Action<string> ControlTypeChangedAction;

		public Vector2 MoveInputVector
        {
			get { return _moveInputVector; }
			set { _moveInputVector = value; }
        }

		public Vector2 LookInputVector
		{
			get { return _lookInputVector; }
			set { _lookInputVector = value; }
		}

		public bool Attack
		{
			get { return _attack; }
			set { _attack = value; }
		}

		public bool Jump
		{
			get { return _jump; }
			set { _jump = value; }
		}

		public bool Sprint
		{
			get { return _sprint; }
			set { _sprint = value; }
		}

		public bool Ready
		{
			get { return _ready; }
			set { _ready = value; }
		}

		public bool Action
        {
			get { return _action; }
			set { _action = value; }
        }

		public void OnMove(InputAction.CallbackContext context)
		{
			_moveInputVector = context.ReadValue<Vector2>();
		}

		public void OnMoveTouch(Vector2 moveDirection)
		{
			_moveInputVector = moveDirection;
		}

		/// <summary>
		/// Input System look input.
		/// </summary>
		/// <param name="context"></param>
		public void OnLook(InputAction.CallbackContext context)
		{
			_lookInputVector = context.ReadValue<Vector2>();
		}

		/// <summary>
		/// Touch screen look input.
		/// </summary>
		/// <param name="lookInput"></param>
		public void OnLookTouch(Vector2 lookInput)
		{
			_lookInputVector = lookInput;
		}

		public void OnAttack(InputAction.CallbackContext context)
		{
			switch (context.phase)
			{
				case InputActionPhase.Performed:
					_attack = true;
					break;
				case InputActionPhase.Canceled:
					_attack = false;
					break;
			}
		}

		public void OnAttackTouch(bool state)
		{
			_attack = state;
		}

		public void OnJump(InputAction.CallbackContext context)
		{
			switch (context.phase)
			{
				case InputActionPhase.Performed:
					_jump = true;
					break;
				case InputActionPhase.Canceled:
					_jump = false;
					break;
			}
		}

		public void OnJumpTouch(bool state)
		{
			_jump = state;
		}

		public void OnSprint(InputAction.CallbackContext context)
		{
			switch (context.phase)
			{
				case InputActionPhase.Performed:
					_sprint = true;
					break;
				case InputActionPhase.Canceled:
					_sprint = false;
					break;
			}
		}

		public void OnSprintTouch(bool state)
		{
			_sprint = state;
		}

		public void OnReady(InputAction.CallbackContext context)
		{
			switch (context.phase)
			{
				case InputActionPhase.Performed:
					_ready = true;
					break;
				case InputActionPhase.Canceled:
					_ready = false;
					break;
			}
		}

		public void OnReadyTouch(bool state)
		{
			_ready = state;
		}

		public void OnAction(InputAction.CallbackContext context)
		{
			switch (context.phase)
			{
				case InputActionPhase.Performed:
					_action = true;
					break;
				case InputActionPhase.Canceled:
					_action = false;
					break;
			}
		}

		public void OnActionTouch(bool state)
		{
			_action = state;
		}

		public void OnControlsChanged(PlayerInput playerInput)
		{
			switch (playerInput.currentControlScheme)
			{
				case "Gamepad":
					currentControlScheme = "Gamepad";
					Debug.LogFormat("Active Device = {0}", currentControlScheme);
					break;

				case "Keyboard&Mouse":
					currentControlScheme = "Keyboard&Mouse";
					Debug.LogFormat("Active Device = {0}", currentControlScheme);
					break;
				case "Touch":
					currentControlScheme = "Touch";
					Debug.LogFormat("Active Device = {0}", currentControlScheme);
					break;
			}

			ControlTypeChangedAction?.Invoke(playerInput.currentControlScheme);
		}
	}
}
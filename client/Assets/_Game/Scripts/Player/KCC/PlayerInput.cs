using UnityEngine;
using Fusion;
using Fusion.KCC;
using UnityEngine.InputSystem;
using Fusion.Sockets;
using System.Collections.Generic;
using System;

namespace BeamableExample.RedlightGreenLight.Character
{
	/// <summary>
	/// Tracks player input for fixed and render updates.
	/// </summary>
	public sealed class PlayerInput : MonoBehaviour, INetworkRunnerCallbacks
	{
		// PUBLIC MEMBERS

		/// <summary>
		/// Indicates the input is available for current frame.
		/// </summary>
		public bool HasInput => _hasInput;

		/// <summary>
		/// Holds input for fixed update.
		/// </summary>
		public GameplayInput FixedInput  => _fixedInput;

		/// <summary>
		/// Holds input for current frame render update.
		/// </summary>
		public GameplayInput RenderInput => _renderInput;

		/// <summary>
		/// Holds combined inputs from all render frames since last fixed update. Used when Fusion input poll is triggered.
		/// </summary>
		public GameplayInput CachedInput => _cachedInput;

		public float lookInputModY = 1.0f;
		public float lookInputModX = 1.0f;
		public bool InvertLookY = true;
		public bool InvertLookX = false;

		// PRIVATE MEMBERS

		private BasePlayer        _player;
		private bool          _hasInput;
		private bool          _trackInput;
		private GameplayInput _fixedInput;
		private GameplayInput _renderInput;
		private GameplayInput _cachedInput;
		private Vector2       _cachedMoveDirection;
		private float         _cachedMoveDirectionSize;
		private bool          _resetCachedInput;
		private int           _moveTouchID;
		private int           _lookTouchID;
		private Vector2       _moveTouchOrigin;
		private Vector2       _lookTouchOrigin;
		private float         _jumpTouchTime;

		// PUBLIC METHODS

		/// <summary>
		/// Register player instance, Fusion input poll is enabled for local player.
		/// </summary>
		public void SetPlayer(BasePlayer player)
		{
			_hasInput    = default;
			_fixedInput  = default;
			_renderInput = default;
			_cachedInput = default;
			_moveTouchID = -1;
			_lookTouchID = -1;

			if (_trackInput == true && _player != null && _player.Runner != null)
			{
				_player.Runner.RemoveCallbacks(this);
			}

			_player     = player;
			_trackInput = player != null && player.IsLocal == true;

			if (_trackInput == true)
			{
				_player.Runner.AddCallbacks(this);

                Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible   = false;

				Debug.LogFormat("{0} has Input SetPlayer()", player.name);
			}
		}

        /// <summary>
        /// 1. Collect input from devices, can be executed multiple times between OnFixedUpdate() calls because of faster rendering speed.
        /// </summary>
        public void OnBeforeUpdate()
		{
			if (_trackInput == false)
				return;

			// Reset input for current frame to default
			_renderInput = default;

			// Cached input was polled and explicit reset requested
			if (_resetCachedInput == true)
			{
				_resetCachedInput = false;

				_cachedInput             = default;
				_cachedMoveDirection     = default;
				_cachedMoveDirectionSize = default;
			}

			// Cursor lock processing
			if (Keyboard.current.tabKey.wasPressedThisFrame == true)
			{
				if (Cursor.lockState == CursorLockMode.Locked)
				{
					Cursor.lockState = CursorLockMode.None;
					Cursor.visible = true;
				}
				else
				{
					Cursor.lockState = CursorLockMode.Locked;
					Cursor.visible = false;
				}
			}

#if !UNITY_ANDROID && !UNITY_IOS
			// Input is tracked only if the cursor is locked
			if (Cursor.lockState != CursorLockMode.Locked)
				return;
#else
			ProcessMobileInput();
#endif

			ProcessInput();
		}

		/// <summary>
		/// 3. Read input from Fusion. On input authority the FixedInput will match CachedInput.
		/// </summary>
		public void OnFixedUpdate()
		{
			_hasInput   = default;
			_fixedInput = default;

			if (_player == null)
				return;

			if (_player.Object.InputAuthority != PlayerRef.None)
			{
				if (_player.Runner.TryGetInputForPlayer(_player.Object.InputAuthority, out GameplayInput input) == true)
				{
					_fixedInput = input;
					_hasInput   = true;
				}
			}
		}

		// PRIVATE METHODS

		/// <summary>
		/// 2. Push cached input and reset properties, can be executed multiple times within single Unity frame if the rendering speed is slower than Fusion simulation (or there is a performance spike).
		/// </summary>
		void INetworkRunnerCallbacks.OnInput(NetworkRunner runner, NetworkInput networkInput)
		{
			GameplayInput gameplayInput = _cachedInput;

			// Input is polled for single fixed update, but at this time we don't know how many times in a row OnInput() will be executed.
			// This is the reason for having a reset flag instead of resetting input immediately, otherwise we could lose input for next fixed updates (for example move direction).

			_resetCachedInput = true;

			// Now we reset all properties which should not propagate into next OnInput() call (for example LookRotationDelta - this must be applied only once and reset immediately).
			// If there's a spike, OnInput() and OnFixedUpdate() will be called multiple times in a row without OnBeforeUpdate() in between, so we don't reset move direction to preserve movement.
			// Instead, move direction and other sensitive properties are reset in next OnBeforeUpdate() - driven by _resetCachedInput.

			_cachedInput.Jumped				= false;
			//_cachedInput.Jumping			= false;
			//_cachedInput.Action			= false;
			_cachedInput.Attack				= false;
			_cachedInput.Ready				= false;
			_cachedInput.LookRotationDelta	= default;

			// Input consumed by OnInput() call will be read in OnFixedUpdate() and immediately propagated to KCC.
			// Here we should reset render properties so they are not applied twice (fixed + render update).

			_renderInput.Jumped				= false;
			//_renderInput.Jumping			= false;
			//_renderInput.Action			= false;
			_renderInput.Attack				= false;
			_renderInput.Ready				= false;
			_renderInput.LookRotationDelta	= default;

			networkInput.Set(gameplayInput);
		}

		private void ProcessInput()
		{
			Vector2 moveDirection		= PlayerInputListener.Instance.MoveInputVector;
			Vector2 lookRotationDelta = new Vector2(
											PlayerInputListener.Instance.LookInputVector.y * ((InvertLookY == true) ? -1.0f : 1.0f) * lookInputModY,
											PlayerInputListener.Instance.LookInputVector.x * ((InvertLookX == true) ? -1.0f : 1.0f) * lookInputModX);

			if (moveDirection.IsZero() == false)
			{
				moveDirection.Normalize();
			}

			// Process input for render, represents current device state.

			_renderInput.MoveDirection		= moveDirection;
			_renderInput.LookRotationDelta	= lookRotationDelta;
			_renderInput.Sprint				= PlayerInputListener.Instance.Sprint;
			_renderInput.Jumping			= PlayerInputListener.Instance.Jump;
			_renderInput.Action				= PlayerInputListener.Instance.Action;
			_renderInput.Jumped				= PlayerInputListener.Instance.Jump;
			_renderInput.Attack				= PlayerInputListener.Instance.Attack;
			_renderInput.Ready				= PlayerInputListener.Instance.Ready;
			
			PlayerInputListener.Instance.Attack = false;
			PlayerInputListener.Instance.Ready = false;
			PlayerInputListener.Instance.Jump = false;

			// Process cached input for next OnInput() call, represents accumulated inputs for all render frames since last fixed update.

			float deltaTime = Time.deltaTime;

			// Move direction accumulation is a special case. Let's say simulation runs 30Hz (33.333ms delta time) and render runs 300Hz (3.333ms delta time).
			// If the player hits W key in last frame before fixed update, the KCC will move in render update by (velocity * 0.003333f).
			// Treating this input the same way for next fixed update results in KCC moving by (velocity * 0.03333f) - 10x more.
			// Following accumulation proportionally scales move direction so it reflects frames in which input was active.
			// This way the next fixed update will correspond more accurately to what happened in render frames.

			_cachedMoveDirection		+= moveDirection * deltaTime;
			_cachedMoveDirectionSize	+= deltaTime;

			_cachedInput.MoveDirection		= _cachedMoveDirection / _cachedMoveDirectionSize;
			_cachedInput.LookRotationDelta	+= _renderInput.LookRotationDelta;
			_cachedInput.Sprint				|= _renderInput.Sprint;
			_cachedInput.Jumping			|= _renderInput.Jumping;
			_cachedInput.Action				|= _renderInput.Action;
			_cachedInput.Jumped				|= _renderInput.Jumped;
			_cachedInput.Attack				|= _renderInput.Attack;
			_cachedInput.Ready				|= _renderInput.Ready;
		}

		private void ProcessMobileInput()
		{
			bool    disableLook		= true;
			Vector2 lookTouchDelta	= Vector2.zero;

			for (int i = 0, count = Input.touchCount; i < count; ++i)
			{
				Touch touch = Input.touches[i];
				if (touch.phase == UnityEngine.TouchPhase.Began)
				{
					if (touch.position.x >= Screen.width * 0.5)
					{
						_lookTouchID     = touch.fingerId;
						_lookTouchOrigin = touch.position;
						_jumpTouchTime   = Time.unscaledTime;
					}
				}

				if (touch.fingerId == _lookTouchID)
				{
					lookTouchDelta = touch.position - _lookTouchOrigin;
					_lookTouchOrigin = touch.position;
					disableLook = false;
				}
			}

			if (disableLook) { _lookTouchID = -1; }

			_renderInput.LookRotationDelta = new Vector2(-lookTouchDelta.y * 0.1f, lookTouchDelta.x * 0.25f);
			_cachedInput.LookRotationDelta		+= _renderInput.LookRotationDelta;
			
		}

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
        }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
        {
        }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
        }

        public void OnConnectedToServer(NetworkRunner runner)
        {
        }

        public void OnDisconnectedFromServer(NetworkRunner runner)
        {
        }

        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
        {
        }

        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
        {
        }

        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
        {
        }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {
        }

        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
        {
        }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data)
        {
        }

        public void OnSceneLoadDone(NetworkRunner runner)
        {
        }

        public void OnSceneLoadStart(NetworkRunner runner)
        {
        }

        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
        {
            throw new NotImplementedException();
        }
    }
}

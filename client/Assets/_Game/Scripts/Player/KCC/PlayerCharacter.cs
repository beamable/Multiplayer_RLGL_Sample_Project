using Beamable;
using Fusion;
using Fusion.KCC;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beamable.Microservices;
using Beamable.Server.Clients;
using UnityEngine;
using static BeamableExample.RedlightGreenLight.GameManager;

namespace BeamableExample.RedlightGreenLight.Character
{
    /// <summary>
    /// Advanced player implementation.
    /// </summary>
    public sealed class PlayerCharacter : AdvancedPlayer, ICanTakeDamage
    {
        // PUBLIC MEMBERS

        public int PlayerRefID { get; private set; }

        public bool IsActivated => (gameObject.activeInHierarchy && (State == PlayerState.Active || State == PlayerState.Safe || State == PlayerState.Spawning));
        public bool IsRespawningDone => State == PlayerState.Spawning && _respawnTimer.Expired(Runner);

        [Networked(OnChanged = nameof(OnStateChanged))]
        public PlayerState State { get; set; }

        [Networked(OnChanged = nameof(OnNameChanged))]
        public string _playerName { get; set; }

        [Networked(OnChanged = nameof(OnScoreChanged))]
        public int gameScore { get; set; }

        [Networked(OnChanged = nameof(OnPlayerHealthChanged))]
        public int _playerHealth { get; set; }

        [Networked]
        public int _playerMaxHealth { get; set; }

        [Networked]
        public float timeOfDeath { get; set; }
        [Networked]
        public int finishedPosition { get; set; }
        [Networked]
        public int totalKills { get; set; }
        public  bool hasBeenSeen { get; set; }

        public ProcessPostGameResultsClient postGameResultsClient { get; set; }

        public WeaponManager WeaponManager => _weaponManager;
        public CharacterAnimationToPhysics characterPhysics;
        public PlayerInfoUI playerInfoUI;
        public Rigidbody rootRigidbody;


        public Action<PlayerCharacter> KilledCallback;
        
        
        

        // PRIVATE MEMBERS

        [Tooltip("The Character Animator")]
        [SerializeField] private NetworkMecanimAnimator networkAnimator;

        [SerializeField]
        [Tooltip("Visual should always face player move direction.")]
        private bool _faceMoveDirection;

        [SerializeField]
        [Tooltip("Events which trigger look rotation update of KCC.")]
        private ELookRotationUpdateSource _lookRotationUpdateSource = ELookRotationUpdateSource.Jump | ELookRotationUpdateSource.Movement;

        [SerializeField]
        [Tooltip("The hold time before a pickup action is triggered.")]
        private float _pickupActionTime = 0.5f;

        [SerializeField]
        [Tooltip("The area around the player that pickups will trigger.")]
        private float _pickupRadius;

        [SerializeField]
        [Tooltip("Layer Mask for pickupable objects.")]
        private LayerMask _pickupMask;

        private Collider[] _pickupOverlaps = new Collider[1];
        private PickupManager _pickupManager;

        [SerializeField]
        [Tooltip("Manages all the weapons for this player.")]
        private WeaponManager _weaponManager;

        [Networked]
        [HideInInspector]
        private VirtualCameraType _activeVirtualCameraType { get; set; }

        [Networked]
        [HideInInspector]
        private Vector2 _pendingLookRotationDelta { get; set; }

        [Networked]
        [HideInInspector]
        private float _facingMoveRotation { get; set; }

        [Networked]
        [HideInInspector]
        private TickTimer _respawnTimer { get; set; }

        [HideInInspector]
        [Networked]
        private TickTimer _pickUpActionTimer { get; set; }

        [Networked(OnChanged = nameof(OnReadyChanged))]
        [HideInInspector]
        private NetworkBool _ready { get; set; }

        private Vector2 _renderLookRotationDelta;
        private Interpolator<float> _facingMoveRotationInterpolator;
        private float _respawnInSeconds = -1;
        private int _activeSpectatorIndex;
        private bool DoFireAnimation = false;
        private string _gameOverMessage;
        private DamageInformation _damageInfo;
        private BeamContext _context;
        private long _beamableId;

        private const string TOTAL_KILLS_KEY = "total_kills";
        private const string KILLS_IN_GAME_KEY = "SEVEN_KILLS_IN_GAME";
        private const int KILLS_IN_GAME_NUMBER = 7;
        private const string MATCHES_PLAYED_KEY = "MATCHES_PLAYED";
        private const string INVISIBLE_FOR_ENTIRE_GAME_KEY = "INVISIBLE_FOR_ENTIRE_GAME";
        private const string FIRST_PLACE_KEY = "FIRST_PLACE";
        private const string MATCHES_WON_KEY  = "MATCHES_WON";

        // AdvancedPlayer INTERFACE
        
        protected override void OnSpawned()
        {
            _facingMoveRotationInterpolator = GetInterpolator<float>(nameof(_facingMoveRotation));

            State = PlayerState.Spawning;

            // Getting this here because it will revert to -1 if the player disconnects, but we still want to remember the Id we were assigned for clean-up purposes
            PlayerRefID = Object.InputAuthority;
            _ready = false;

            PlayerManager.AddPlayer(this);

            playerInfoUI.SetLookTarget(CinemachineManager.Instance.thirdPersonCamera.transform);

            if (Object.HasInputAuthority)
            {
                CinemachineManager.Instance.thirdPersonCamera.Follow = CameraPivot;

                SetActiveVirtualCamera(VirtualCameraType.FOLLOW);
                
                SetUpBeamable();
                GetBeameableAlias();
                
                hasBeenSeen = false;
            }

            if (Object.HasStateAuthority)
            {
                WeaponManager.WeaponChangedEvent += PlayerHUDInfoUI.Instance.SetWeaponIconUI;

                SetPlayerHealth(_playerMaxHealth);
            }

            postGameResultsClient = new ProcessPostGameResultsClient();
        }

        private async void GetBeameableAlias()
        {
            var stats = await _context.Api.StatsService.GetStats("client", "public", "player", _beamableId);
            string alias = "Guest";
            if (stats.ContainsKey("alias"))
                alias = stats["alias"];

            RPC_SetPlayerName(alias);
        }

        private async void SetUpBeamable()
        {
            _context = BeamContext.Default;
            await _context.OnReady;
            _beamableId = _context.Api.User.id;
            BeamableStatsController.SetUpBeamable();
        }

        [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.StateAuthority, InvokeLocal = true, Channel = RpcChannel.Reliable)]
        public void RPC_SetPlayerName(string name)
        {
            Debug.LogFormat("<color=green>Player_{0} RPC_SetPlayerName()</color>", PlayerRefID);

            _playerName = name;
            playerInfoUI.SetNameText(name);
        }

        private static void OnNameChanged(Changed<PlayerCharacter> changed)
        {
            Debug.LogFormat("<color=green>Player_{0} OnNameChanged = {1}</color>", changed.Behaviour.Object.InputAuthority, changed.Behaviour._playerName);

            changed.Behaviour.playerInfoUI.SetNameText(changed.Behaviour._playerName);
        }

        // 1.
        protected override void ProcessEarlyFixedInput()
        {
            // Here we process input and set properties related to movement / look.
            // For following lines, we should use Input.FixedInput only. This property holds input for fixed updates.

            switch (State)
            {
                case PlayerState.Active:
                case PlayerState.Safe:
                    {
                        switch (_activeVirtualCameraType)
                        {
                            case VirtualCameraType.FOLLOW:
                                {
                                    // Clamp input look rotation delta. Instead of applying immediately to KCC, we store it locally as pending and defer application to a point where conditions for application are met.
                                    // This allows us to rotate with camera around character standing still.
                                    Vector2 lookRotation = KCC.FixedData.GetLookRotation(true, true);
                                    _pendingLookRotationDelta = KCCUtility.GetClampedLookRotationDelta(lookRotation, _pendingLookRotationDelta + Input.FixedInput.LookRotationDelta, -MaxCameraAngle, MaxCameraAngle);
                                }
                                break;
                        }


                        bool updateLookRotation = default;
                        Quaternion facingRotation = default; // default is invalid (not set)
                        Quaternion jumpRotation = default; // default is invalid (not set)

                        // Checking look rotation update conditions
                        if (HasLookRotationUpdateSource(ELookRotationUpdateSource.Jump) == true)
                        {
                            updateLookRotation |= Input.FixedInput.Jumping;
                        }
                        if (HasLookRotationUpdateSource(ELookRotationUpdateSource.Movement) == true)
                        {
                            updateLookRotation |= Input.FixedInput.MoveDirection.IsZero() == false;
                        }
                        if (HasLookRotationUpdateSource(ELookRotationUpdateSource.MouseMovement) == true)
                        {
                            updateLookRotation |= Input.FixedInput.LookRotationDelta.IsZero() == false;
                        }

                        if (updateLookRotation == true)
                        {
                            // Some conditions are met, we can apply pending look rotation delta to KCC
                            if (_pendingLookRotationDelta.IsZero() == false)
                            {
                                KCC.AddLookRotation(_pendingLookRotationDelta);
                                _pendingLookRotationDelta = default;
                            }
                        }

                        if (updateLookRotation == true || _faceMoveDirection == false)
                        {
                            // Setting base facing and jump rotation
                            facingRotation = KCC.FixedData.TransformRotation;
                            jumpRotation = KCC.FixedData.TransformRotation;
                        }

                        Vector3 inputDirection = default;

                        Vector3 moveDirection = Input.FixedInput.MoveDirection.X0Y();
                        if (moveDirection.IsZero() == false)
                        {
                            // Calculating world space input direction for KCC, update facing and jump rotation based on configuration.

                            switch (_activeVirtualCameraType)
                            {
                                case VirtualCameraType.FOLLOW:
                                    {
                                        inputDirection = KCC.FixedData.TransformRotation * moveDirection;
                                    }
                                    break;
                            }


                            Quaternion inputRotation = Quaternion.LookRotation(inputDirection);

                            // We are moving in certain direction, we'll use it also for jump.
                            jumpRotation = inputRotation;

                            // Facing move direction enabled and right mouse button rotation lock disabled? Treat input rotation as facing as well.
                            if (_faceMoveDirection == true)
                            {
                                facingRotation = inputRotation;
                            }
                        }

                        KCC.SetInputDirection(inputDirection);

                        if (Input.FixedInput.Jumping == true)
                        {
                            // Is jump rotation invalid (not set)? Get it from other source.
                            if (jumpRotation.IsZero() == true)
                            {
                                // Is facing rotation valid? Use it.
                                if (facingRotation.IsZero() == false)
                                {
                                    jumpRotation = facingRotation;
                                }
                                else
                                {
                                    // Otherwise just jump forward.
                                    jumpRotation = KCC.FixedData.TransformRotation;
                                }
                            }

                            // Is facing rotation invalid (not set)? Set it to the same rotation as jump.
                            if (facingRotation.IsZero() == true)
                            {
                                facingRotation = jumpRotation;
                            }

                            KCC.Jump(jumpRotation * JumpImpulse);
                        }

                        //networkAnimator.Animator.SetBool("Grounded", KCC.FixedData.IsGrounded);

                        networkAnimator.Animator.SetFloat("Speed", KCC.Data.KinematicVelocity.magnitude / 8.0f);

                        networkAnimator.Animator.SetFloat("HorizontalSpeed", Input.FixedInput.MoveDirection.x);
                        networkAnimator.Animator.SetFloat("VerticalSpeed", Input.FixedInput.MoveDirection.y);

                        // Notice we are checking KCC.FixedData because we are in fixed update code path (render update uses KCC.RenderData)
                        if (KCC.FixedData.IsGrounded == true)
                        {
                            // Sprint is updated only when grounded
                            KCC.SetSprint(Input.FixedInput.Sprint);
                        }

                        // Is facing rotation set? Apply to the visual and store it.
                        if (facingRotation.IsZero() == false)
                        {
                            Visual.transform.rotation = facingRotation;

                            if (_faceMoveDirection == true)
                            {
                                KCCUtility.GetLookRotationAngles(facingRotation, out float facingPitch, out float facingYaw);
                                _facingMoveRotation = facingYaw;
                            }
                        }

                        // Other movement related actions here (crouch, ...)
                    }
                    break;

                case PlayerState.Spectator:
                    {
                        // Clear kinematic velocity entirely
                        KCC.SetKinematicVelocity(Vector3.zero);

                        // Clear dynamic velocity proportionaly to impulse direction
                        KCC.SetDynamicVelocity(Vector3.zero);

                        // Clear out the input direction.
                        KCC.SetInputDirection(Vector3.zero);
                    }
                    break;
                case PlayerState.Dead:
                    {
                        // Clear kinematic velocity entirely
                        KCC.SetKinematicVelocity(Vector3.zero);

                        // Clear dynamic velocity proportionaly to impulse direction
                        KCC.SetDynamicVelocity(Vector3.zero);

                        // Clear out the input direction.
                        KCC.SetInputDirection(Vector3.zero);
                    }
                    break;
            }
        }

        // 2.
        protected override void OnFixedUpdate()
        {
            // Regular fixed update for Player/AdvancedPlayer class.
            // Executed after all player KCC updates and before HitboxManager.

            switch (State)
            {
                case PlayerState.Active:
                case PlayerState.Safe:
                    {
                        // Setting camera pivot location
                        // In this case we have to apply pending look rotation delta (cached locally) on top of current KCC look rotation.

                        switch (_activeVirtualCameraType)
                        {
                            case VirtualCameraType.FOLLOW:
                                {
                                    Vector2 pitchRotation = KCC.FixedData.GetLookRotation(true, false);
                                    Vector2 clampedCameraRotation = KCCUtility.GetClampedLookRotation(pitchRotation + _pendingLookRotationDelta, -MaxCameraAngle, MaxCameraAngle);

                                    CameraPivot.rotation = KCC.FixedData.TransformRotation * Quaternion.Euler(clampedCameraRotation);
                                }
                                break;
                        }


                        if (_faceMoveDirection == true && Object.HasInputAuthority == false && Object.HasStateAuthority == false)
                        {
                            // Facing rotation for visual is already set on input and state authority, here we update proxies based on [Networked] property.
                            Visual.transform.rotation = Quaternion.Euler(0.0f, _facingMoveRotation, 0.0f);
                        }

                        CheckForPowerupPickup();
                    }
                    break;
                case PlayerState.Spectator:
                    {
                    }
                    break;
            }

            if (Object.HasStateAuthority)
            {
                if (_respawnInSeconds >= 0)
                    CheckRespawn();

                if (IsRespawningDone)
                    ResetPlayer();
            }
        }

        // 3.
        protected override void ProcessLateFixedInput()
        {
            switch (State)
            {
                case PlayerState.Active:
                    {
                        // Executed after HitboxManager. Process other non-movement actions like shooting.

                        if (Input.FixedInput.Attack == true)
                        {
                            Attack();
                        }

                        if (Input.FixedInput.Ready == true)
                        {
                            SetReady(true);
                        }

                        if (Input.FixedInput.Jumped == true)
                        {
                            Jump();
                        }
                    }
                    break;
                case PlayerState.Safe:
                    {
                        if (Input.FixedInput.Jumped == true)
                        {
                            Jump();
                        }
                    }
                    break;
                case PlayerState.Spectator:
                    {
                    }
                    break;
            }

            networkAnimator.Animator.SetBool("Grounded", KCC.FixedData.IsGrounded);
            
            if (DoFireAnimation)
            {
                FireWeapon();
                DoFireAnimation = false;
            }
        }

        // 4.
        protected override void ProcessRenderInput()
        {
            // Here we process input and set properties related to movement / look.
            // For following lines, we should use Input.RenderInput and Input.CachedInput only. These properties hold input for render updates.
            // Input.RenderInput holds input for current render frame.
            // Input.CachedInput holds combined input for all render frames from last fixed update. This property will be used to set input for next fixed update (polled by Fusion).

            switch (State)
            {
                case PlayerState.Active:
                case PlayerState.Safe:
                    {
                        Vector2 lookRotation = Vector2.zero;

                        switch (_activeVirtualCameraType)
                        {
                            case VirtualCameraType.FOLLOW:
                                {
                                    // Get look rotation from last fixed update (not last render!)
                                    lookRotation = KCC.FixedData.GetLookRotation(true, true);

                                    // For correct look rotation, we have to apply deltas from all render frames since last fixed update => stored in Input.CachedInput
                                    // Additionally we have to apply pending look rotation delta maintained in fixed update, resulting in pending look rotation delta dedicated to render update.
                                    _renderLookRotationDelta = KCCUtility.GetClampedLookRotationDelta(lookRotation, _pendingLookRotationDelta + Input.CachedInput.LookRotationDelta, -MaxCameraAngle, MaxCameraAngle);
                                }
                                break;
                        }

                        bool updateLookRotation = default;
                        Quaternion facingRotation = default;
                        Quaternion jumpRotation = default;

                        // Checking look rotation update conditions. These check are done agains Input.CachedInput, because any render input accumulated since last fixed update will trigger look rotation update in next fixed udpate.
                        if (HasLookRotationUpdateSource(ELookRotationUpdateSource.Jump) == true)
                        {
                            updateLookRotation |= Input.CachedInput.Jumping == true;
                        }

                        if (HasLookRotationUpdateSource(ELookRotationUpdateSource.Movement) == true)
                        {
                            updateLookRotation |= Input.CachedInput.MoveDirection.IsZero() == false;
                        }

                        if (HasLookRotationUpdateSource(ELookRotationUpdateSource.MouseMovement) == true)
                        {
                            updateLookRotation |= Input.CachedInput.LookRotationDelta.IsZero() == false;
                        }

                        if (updateLookRotation == true)
                        {
                            // Some conditions are met, we can apply pending render look rotation delta to KCC
                            if (_renderLookRotationDelta.IsZero() == false)
                            {
                                switch (_activeVirtualCameraType)
                                {
                                    case VirtualCameraType.FOLLOW:
                                        {
                                            KCC.SetLookRotation(lookRotation + _renderLookRotationDelta);
                                        }
                                        break;
                                }

                            }
                        }

                        if (updateLookRotation == true || _faceMoveDirection == false)
                        {
                            // Setting base facing and jump rotation
                            facingRotation = KCC.RenderData.TransformRotation;
                            jumpRotation = KCC.RenderData.TransformRotation;
                        }

                        Vector3 inputDirection = default;
                        bool hasInputDirection = default;

                        // Do we have move direction for this render frame? Use it.
                        if (Input.RenderInput.MoveDirection.IsZero() == false)
                        {
                            switch (_activeVirtualCameraType)
                            {
                                case VirtualCameraType.FOLLOW:
                                    {
                                        inputDirection = KCC.RenderData.TransformRotation * new Vector3(Input.RenderInput.MoveDirection.x, 0.0f, Input.RenderInput.MoveDirection.y);
                                    }
                                    break;
                            }

                            hasInputDirection = true;
                        }

                        KCC.SetInputDirection(inputDirection);

                        // There is no move direction for current render input. Do we have cached move direction (accumulated in frames since last fixed update)? Then use it. It will be used next fixed update after Fusion polls new input.
                        if (hasInputDirection == false && Input.CachedInput.MoveDirection.IsZero() == false)
                        {
                            inputDirection = KCC.RenderData.TransformRotation * new Vector3(Input.CachedInput.MoveDirection.x, 0.0f, Input.CachedInput.MoveDirection.y);
                            hasInputDirection = true;
                        }

                        // Do we have any input direction (from this frame or all frames since last fixed update)? Use it.
                        if (hasInputDirection == true)
                        {
                            Quaternion inputRotation = Quaternion.LookRotation(inputDirection);

                            // We are moving in certain direction, we'll use it also for jump.
                            jumpRotation = inputRotation;

                            // Facing move direction enabled and right mouse button rotation lock disabled? Treat input rotation as facing as well.
                            if (_faceMoveDirection == true)
                            {
                                facingRotation = inputRotation;
                            }
                        }

                        // Jump is extrapolated for render as well.
                        // Checking Input.CachedInput here. Jump accumulated from render inputs since last fixed update will trigger similar code next fixed update.
                        // We have to keep the visual to face the direction if there is a jump pending execution in fixed update.
                        if (Input.CachedInput.Jumping == true)
                        {
                            // Is jump rotation invalid (not set)? Get it from other source.
                            if (jumpRotation.IsZero() == true)
                            {
                                // Is facing rotation valid? Use it.
                                if (facingRotation.IsZero() == false)
                                {
                                    jumpRotation = facingRotation;
                                }
                                else
                                {
                                    // Otherwise just jump forward.
                                    jumpRotation = KCC.RenderData.TransformRotation;
                                }
                            }

                            // Is facing rotation invalid (not set)? Set it to the same rotation as jump.
                            if (facingRotation.IsZero() == true)
                            {
                                facingRotation = jumpRotation;
                            }

                            // Jumping only when Input.RenderInput is set (checking Input.CachedInput might cause glitches)
                            if (Input.RenderInput.Jumping == true)
                            {
                                KCC.Jump(jumpRotation * JumpImpulse);
                            }
                        }

                        // Notice we are checking KCC.RenderData because we are in render update code path (fixed update uses KCC.FixedData)
                        if (KCC.RenderData.IsGrounded == true)
                        {
                            // Sprint is updated only when grounded
                            KCC.SetSprint(Input.CachedInput.Sprint);
                        }

                        // Is facing rotation set? Apply to the visual.
                        if (facingRotation.IsZero() == false)
                        {
                            Visual.transform.rotation = facingRotation;
                        }

                        // At this point, KCC haven't been updated yet (except look rotation, which propagates immediately) so camera have to be synced later.
                        // Base class triggers manual KCC update immediately after this method.
                        // This allows us to synchronize camera in OnEarlyRender(). To keep consistency with fixed update, camera related properties are updated in regular render update - OnRender().
                    }
                    break;
                case PlayerState.Spectator:
                    {
                        // Clear kinematic velocity entirely
                        KCC.SetKinematicVelocity(Vector3.zero);

                        // Clear dynamic velocity proportionaly to impulse direction
                        KCC.SetDynamicVelocity(Vector3.zero);

                        // Clear out the input direction.
                        KCC.SetInputDirection(Vector3.zero);
                    }
                    break;
                case PlayerState.Dead:
                    {
                        // Clear kinematic velocity entirely
                        KCC.SetKinematicVelocity(Vector3.zero);

                        // Clear dynamic velocity proportionaly to impulse direction
                        KCC.SetDynamicVelocity(Vector3.zero);

                        // Clear out the input direction.
                        KCC.SetInputDirection(Vector3.zero);
                    }
                    break;
            }
        }

        // 5.
        protected override void OnRender()
        {
            CullingManager.UpdateCullingBounds();

            switch (State)
            {
                case PlayerState.Active:
                    {
                        RenderPlayerMotion();
                    }
                    break;
                case PlayerState.Safe:
                    {
                        RenderPlayerMotion();
                    }
                    break;
                case PlayerState.Spectator:
                    {
                    }
                    break;
            }

        }

        private void RenderPlayerMotion()
        {
            // For render we care only about input authority.
            // This can be extended to state authority if needed (inner code won't be executed on host for other players, having camera pivots to be set only from fixed update, causing jitter if spectating that player)
            if (Object.HasInputAuthority == true)
            {
                switch (_activeVirtualCameraType)
                {
                    case VirtualCameraType.FOLLOW:
                        {
                            Vector2 pitchRotation = KCC.FixedData.GetLookRotation(true, false);
                            Vector2 clampedCameraRotation = KCCUtility.GetClampedLookRotation(pitchRotation + _renderLookRotationDelta, -MaxCameraAngle, MaxCameraAngle);

                            CameraPivot.rotation = KCC.FixedData.TransformRotation * Quaternion.Euler(clampedCameraRotation);
                        }
                        break;
                }
            }

            if (_faceMoveDirection == true && Object.HasInputAuthority == false)
            {
                // Facing rotation for visual is already set on input authority, here we update proxies and state authority based on [Networked] property.

                float interpolatedFacingMoveRotation = _facingMoveRotation;

                if (_facingMoveRotationInterpolator.TryGetValues(out float fromFacingMoveRotation, out float toFacingMoveRotation, out float alpha) == true)
                {
                    // Interpolation which correctly handles circular range (-180 => 180)
                    interpolatedFacingMoveRotation = KCCNetworkUtility.InterpolateRange(fromFacingMoveRotation, toFacingMoveRotation, -180.0f, 180.0f, alpha);
                }

                Visual.transform.rotation = Quaternion.Euler(0.0f, interpolatedFacingMoveRotation, 0.0f);
            }
        }

        // PUBLIC METHODS

        public void InitNetworkState()
        {
            State = PlayerState.New;
        }

        public void SetReady(bool value)
        {
            _ready = value;
        }

        public NetworkBool IsReady()
        {
            return _ready;
        }

        private static void OnReadyChanged(Changed<PlayerCharacter> changed)
        {
            if (changed.Behaviour.Object.HasInputAuthority)
            {
                Debug.LogFormat("<color=blue>Player_{0} Ready = {1}</color>", changed.Behaviour.Object.InputAuthority, changed.Behaviour._ready);
            }
        }

        public void SetPlayerHealth(int value)
        {
            _playerHealth = value;
        }

        public int GetPlayerHealth()
        {
            return _playerHealth;
        }

        public int SubtractPlayerHealth(int value)
        {
            _playerHealth = _playerHealth - value;
            return _playerHealth;
        }

        public int AddPlayerHealth(int value)
        {
            _playerHealth = _playerHealth + value;
            return _playerHealth;
        }

        /// <summary>
        /// This will get called on the owner client and all the clients clones when _playerHealth is changed.
        /// </summary>
        /// <param name="changed"></param>
        private static void OnPlayerHealthChanged(Changed<PlayerCharacter> changed)
        {
            // Get the changed value.
            int newHealth = changed.Behaviour.GetPlayerHealth();

            // Example how to get old and new values.
            changed.LoadOld();
            int prevHealth = changed.Behaviour.GetPlayerHealth();

            if (changed.Behaviour)
                changed.Behaviour.OnPlayerHealthChanged(newHealth);
        }

        public void OnPlayerHealthChanged(int newHealth)
        {
            // The OnChanged call happens on all client's clones that means we have to make sure to only upate the UI of the "owner" client. If we do not than the UI will update for all clients with this clients health value.
            if (Object.HasInputAuthority)
            {
                // Update the UI with the players health.
                PlayerHUDInfoUI.Instance.UpdateHealthUI(newHealth);
            }
        }

        [Rpc(sources: RpcSources.All, targets: RpcTargets.All, InvokeLocal = true, Channel = RpcChannel.Reliable)]
        public void RPC_KillPlayer(DisplayState displayState, KillSource killSource, string reason)
        {
            Debug.LogFormat("<color=green>Player_{0} RPC_KillPlayer()</color>", PlayerRefID);

            KillPlayer(displayState, killSource, reason);
        }

        private void KillPlayer(DisplayState displayState, KillSource killSource, string reason)
        {
            Debug.LogFormat("<color=green>Player_{0} KillPlayer()</color>", PlayerRefID);

            switch (killSource)
            {
                case KillSource.PLAYER:
                    {
                        networkAnimator.Animator.SetTrigger("DeathByPlayer");
                    }
                    break;
                case KillSource.LASER:
                    {
                        networkAnimator.Animator.SetTrigger("DeathByLaser");
                        networkAnimator.Animator.SetFloat("BaseSpeed", 0.0f);

                        var airProcessor = this.GetComponent<AirKCCProcessor>();
                        if (airProcessor != null)
                        {
                            airProcessor.SetGravityModifier(0.0f);
                        }
                    }
                    break;
                case KillSource.ENDOFGAME:
                    {
                        // TODO:: Put the correct death anim trigger here.
                        networkAnimator.Animator.SetTrigger("DeathByPlayer");
                    }
                    break;
            }



            State = PlayerState.Dead;
            
            // We need to destroy any weapons in the players hand.
            WeaponManager.ClearWeapons();
            SetPlayerHealth(0);

            if (Object.HasStateAuthority)
            {
                // Update this players stats.
                timeOfDeath = GameManager.Instance.levelManager.redLightManager.GetMatchTimeLeft();
            }

            _gameOverMessage = reason;

            if (Object.HasInputAuthority)
            {
                switch (displayState)
                {
                    case GameManager.DisplayState.ELIMINATION:
                        {
                            // Show the EliminatedUI after a few seconds.
                            StartCoroutine(DisplayEliminatedUI(reason));
                        }
                        break;
                    case GameManager.DisplayState.COMPLETION:
                        {
                            // Show the Game Completion after a few seconds.
                            DisplayMatchCompletionUI(reason);
                        }
                        break;
                }
            }
        }

        private IEnumerator DisplayEliminatedUI(string reason)
        {
            yield return new WaitForSeconds(1.0f);

            var eliminationUI = FindObjectOfType<EliminationUI>(true);
            if (eliminationUI == null)
                yield break;

            eliminationUI.Initialize(timeOfDeath, gameScore, reason, true);
        }

        [Rpc(sources: RpcSources.All, targets: RpcTargets.All, InvokeLocal = true, Channel = RpcChannel.Reliable)]
        public void RPC_Celebrate(string reason)
        {
            Debug.LogFormat("<color=green>Player_{0} RPC_Celebrate()</color>", PlayerRefID);

            Celebrate(reason);
        }

        private async void Celebrate(string reason)
        {
            Debug.LogFormat("<color=green>Player_{0} Celebrate()</color>", PlayerRefID);
            
            networkAnimator.Animator.SetTrigger("Celebrate");
            State = PlayerState.Dead;
            if (Object.HasInputAuthority)
            {
                if (!hasBeenSeen)
                {
                    await BeamableStatsController.ChangeStat(INVISIBLE_FOR_ENTIRE_GAME_KEY, "True");
                }
                // Show the EliminatedUI after a few seconds.
                DisplayMatchCompletionUI(reason);
            }
        }

        [Rpc(sources: RpcSources.All, targets: RpcTargets.All, InvokeLocal = true, Channel = RpcChannel.Reliable)]
        public void RPC_DisplayMatchCompletionUI()
        {
            try
            {
                if (!Object.HasInputAuthority)
                    return;
                Debug.LogFormat("<color=green>Player_{0} RPC_DisplayMatchCompletionUI()</color>", PlayerRefID);

                // Show the EliminatedUI after a few seconds.
                DisplayMatchCompletionUI(_gameOverMessage);
            }
            catch (Exception e)
            {
                Debug.LogWarning(e.Message);
            }
        }

        private bool _isMatchCompleteShown = false;
        private async void DisplayMatchCompletionUI(string reason)
        {
            if (_isMatchCompleteShown)
                return;

            _gameOverMessage = reason;

            _isMatchCompleteShown = true;

            var result = new PostGameResult();
            if (Object.HasInputAuthority)
            {
                try
                {
                    result = await postGameResultsClient.ProcessResults(gameScore, finishedPosition, totalKills);
                }
                catch(Exception e)
                {
                    Debug.LogError(e);
                    result = new PostGameResult()
                    {
                        Rewards = new List<Reward>(),
                        Score = 0,
                        GamesPlayed = 0
                    };
                }
                if (totalKills >= KILLS_IN_GAME_NUMBER)
                {
                    await BeamableStatsController.ChangeStat(KILLS_IN_GAME_KEY, "True");
                }
                await BeamableStatsController.AddToStat(MATCHES_PLAYED_KEY, 1);
            }
            
            var matchTime = GameManager.Instance.levelManager.redLightManager.GetMatchTimeLeft() -
                            GameManager.Instance.levelManager.redLightManager.matchTime;
            TrackEvent(matchTime, gameScore);

            await Task.Delay(2000);

            var matchCompletionUI = FindObjectOfType<MatchCompletedUI>(true);
            if (matchCompletionUI == null)
                return;

            // We need to hide the elimination UI if needed.
            var eliminationUI = FindObjectOfType<EliminationUI>(true);
            if (eliminationUI != null)
                eliminationUI.Show(false);

            var spectatorUI = FindObjectOfType<SpectatorUI>(true);
            if (spectatorUI != null)
                spectatorUI.Initialize(false);

            matchCompletionUI.Initialize(result, timeOfDeath, gameScore, reason, true);
        }

        public void EnterSpectatorMode()
        {
            // Open the SpectatorUI.
            var spectatorUI = FindObjectOfType<SpectatorUI>(true);
            if (spectatorUI != null)
            {
                spectatorUI.ChangeSpectatorCallback += ChangeActiveSpectator;
                spectatorUI.Initialize(true);
            }

            SetActiveVirtualCamera(VirtualCameraType.SPECTATOR);

            var trackedIndex = PlayerManager.GetFirstAlivePlayerIndex();
            var trackedPlayer = PlayerManager.GetPlayerFromID(trackedIndex);
            if (trackedPlayer != null)
            {
                _activeSpectatorIndex = trackedIndex;
                CinemachineManager.Instance.spectatorCamera.LookAt = trackedPlayer.transform;
            }
            else
            {
                /// TODO:: If their are no players left alive what should the camera look at.
            }
        }

        private void ChangeActiveSpectator(int direction)
        {
            var trackedIndex = PlayerManager.GetNextAlivePlayerByIndex(_activeSpectatorIndex, direction);
            var trackedPlayer = PlayerManager.GetPlayerFromID(trackedIndex);
            if (trackedPlayer != null)
            {
                _activeSpectatorIndex = trackedPlayer.PlayerRefID;
                CinemachineManager.Instance.spectatorCamera.LookAt = trackedPlayer.transform;
            }
            else
            {
                /// TODO:: If their are no players left alive what should the camera look at.
            }

        }

        public static void OnScoreChanged(Changed<PlayerCharacter> changed)
        {
            if (changed.Behaviour)
            {
                changed.Behaviour.ScoreChanged();
            }
        }

        public void ScoreChanged()
        {
            Debug.LogFormat("<color=green>Player_{0} score = {1}</color>", PlayerRefID, gameScore);
        }

        // This should only be called on the Host/Server.
        public void SetPlayerSafe()
        {
            if (Object.HasStateAuthority)
            {
                // Update this players stats.
                timeOfDeath = GameManager.Instance.levelManager.redLightManager.GetMatchTimeLeft();

                Debug.LogFormat("<color=blue>Player {0} is safe - time = {1}</color>", PlayerRefID, timeOfDeath);

                State = PlayerState.Safe;

                var highlightColor = ColorUtility.ToHtmlStringRGB(FeedManager.Instance.highlightColor);
                var message = $"<color=#{highlightColor}>{_playerName}</color> is <color=#{highlightColor}>safe</color>.";
                RPC_ShowFeedMessage(message);
            }
        }

        [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All, InvokeLocal = true, Channel = RpcChannel.Reliable)]
        public void RPC_ShowFeedMessage(string message)
        {
            FeedManager.Instance.AddFeed(message);
        }

        public static void OnStateChanged(Changed<PlayerCharacter> changed)
        {
            if (changed.Behaviour)
                changed.Behaviour.OnStateChanged();
        }

        public void OnStateChanged()
        {
            switch (State)
            {
                case PlayerState.Active:
                    Debug.LogFormat("PlayerRefID = {0} is Active", PlayerRefID);
                    break;
                case PlayerState.Dead:
                    Debug.LogFormat("PlayerRefID = {0} is Dead", PlayerRefID);
                    break;
                case PlayerState.Despawned:
                    Debug.LogFormat("PlayerRefID = {0} is Despawned", PlayerRefID);
                    break;
                case PlayerState.Disabled:
                    Debug.LogFormat("PlayerRefID = {0} is Disabled", PlayerRefID);
                    break;
                case PlayerState.New:
                    Debug.LogFormat("PlayerRefID = {0} is New", PlayerRefID);
                    break;
                case PlayerState.Safe:
                    Debug.LogFormat("<color=blue>PlayerRefID = {0} is Safe</color>", PlayerRefID);
                    break;
                case PlayerState.Spawning:
                    Debug.LogFormat("PlayerRefID = {0} is Spawning", PlayerRefID);
                    break;
                case PlayerState.Spectator:
                    Debug.LogFormat("PlayerRefID = {0} is Spectator", PlayerRefID);
                    break;
            }
        }

        public void Attack()
        {
            if (Object.HasInputAuthority)
            {
                if (Runner.IsForward)
                {
                    RPC_SetAnimationTrigger("Attack");
                }
            }

        }

        public void FireWeaponFromAnimation()
        {
            DoFireAnimation = true;
        }

        public void FireWeapon()
        {
            if (Object.HasStateAuthority)
            {
                WeaponManager.FireWeapon();
            }
        }

        public void Jump()
        {
            if (Object.HasInputAuthority)
            {
                if (Runner.IsForward)
                {
                    RPC_SetAnimationTrigger("Jump");
                }
            }
        }

        [Rpc(sources: RpcSources.All, targets: RpcTargets.All, InvokeLocal = true, Channel = RpcChannel.Reliable)]
        public void RPC_SetAnimationTrigger(string trigger)
        {
            networkAnimator.Animator.SetTrigger(trigger);
        }

        [Rpc(sources: RpcSources.All, targets: RpcTargets.StateAuthority, InvokeLocal = true, Channel = RpcChannel.Reliable)]
        public void RPC_SetDamageInfo(int damage, KillSource source, WeaponType weapon, DamageModifiers damageMods, PlayerRef attackingPlayerRef)
        {
            if (!Object.HasStateAuthority)
                return;

            SetDamageInfo(damage, source, weapon, damageMods, attackingPlayerRef);
        }

        /// <summary>
        /// Apply damage to Player with an associated impact impulse This should only be called from StateAuth/Host/Server.
        /// </summary>
        /// <param name="damage"></param>
        /// <param name="source"></param>
        /// <param name="weapon"></param>
        /// <param name="damageMods"></param>
        /// <param name="attackingPlayerRef"></param>
        public void SetDamageInfo(int damage, KillSource source, WeaponType weapon, DamageModifiers damageMods, PlayerRef attackingPlayerRef)
        {
            _damageInfo = new DamageInformation
            {
                amount = damage,
                killSource = source,
                weaponType = weapon,
                damageMods = damageMods,
                attackingPlayerRef = attackingPlayerRef
            };

            Debug.LogFormat("RPC_SetDamageInfo({0}, {1}, {2}, {3})", damage, source, weapon, attackingPlayerRef);
        }

        /// <summary>
        /// Apply damage to Player with an associated impact impulse This should only be called from StateAuth/Host/Server.
        /// </summary>
        /// <param name="impulse"></param>
        /// <param name="damage"></param>
        /// <param name="damageMods"></param>
        /// <param name="weaponType"></param>
        /// <param name="source"></param>
        public async void ApplyDamage(Vector3 impulse, int damage, DamageModifiers damageMods, WeaponType weaponType, PlayerRef source)
        {
            if (!IsActivated)
                return;

            // Don't damage yourself
            PlayerCharacter attackingPlayer = PlayerManager.Get(source);
            if (attackingPlayer != null && attackingPlayer.PlayerRefID == PlayerRefID)
                return;

            SetDamageInfo(damage, KillSource.PLAYER, weaponType, damageMods, source);

            int newHealth = SubtractPlayerHealth(damage);
            if (newHealth <= 0)
            {
                var highlightColor = ColorUtility.ToHtmlStringRGB(FeedManager.Instance.highlightColor);
                var reason = $"Killed by <color=#{highlightColor}>{attackingPlayer._playerName}</color> with a <color=#{highlightColor}>{weaponType}</color>";

                // Send out the word to kill the player. The player has StateAuth.
                RPC_KillPlayer(GameManager.DisplayState.ELIMINATION, GameManager.KillSource.PLAYER, reason);

                // Update the attacking players stats.
                attackingPlayer.gameScore += GameManager.Instance.levelManager.redLightManager.CaluculatePointsForKill();
                attackingPlayer.totalKills++;
                await BeamableStatsController.AddToStat(TOTAL_KILLS_KEY, 1);

                var feedMessage = $"<color=#{highlightColor}>{attackingPlayer._playerName}</color> Killed <color=#{highlightColor}>{_playerName}</color> with a <color=#{highlightColor}>{weaponType}</color>";
                RPC_ShowFeedMessage(feedMessage);
            }
            else
            {
                RPC_SetAnimationTrigger("Damage");

                if (damageMods.HasFlag(DamageModifiers.KNOCKBACK))
                    ApplyImpulse(impulse);
            }
        }

        /// <summary>
        /// Apply an impulse to the Player.
        /// </summary>
        /// <param name="impulse">Size and direction of the impulse</param>
        public void ApplyImpulse(Vector3 impulse)
        {
            if (!IsActivated)
                return;

            if (Object.HasStateAuthority)
            {
                // Clear kinematic velocity entirely
                KCC.SetKinematicVelocity(Vector3.zero);

                // Clear dynamic velocity proportionaly to impulse direction
                KCC.SetDynamicVelocity(KCC.Data.DynamicVelocity - Vector3.Scale(KCC.Data.DynamicVelocity, impulse.normalized));

                // Add impulse
                KCC.AddExternalImpulse(impulse);
            }
        }

        public void Respawn(float inSeconds)
        {
            _respawnInSeconds = inSeconds;
        }

        public async void TriggerDespawn()
        {
            DespawnPlayer();
            PlayerManager.RemovePlayer(this);

            await Task.Delay(300); // wait for effects

            if (Object == null) { return; }

            if (Object.HasStateAuthority)
            {
                Runner.Despawn(Object);
            }
            else if (Runner.IsSharedModeMasterClient)
            {
                Object.RequestStateAuthority();

                while (Object.HasStateAuthority == false)
                {
                    await Task.Delay(100); // wait for Auth transfer
                }

                if (Object.HasStateAuthority)
                {
                    Runner.Despawn(Object);
                }
            }
        }

        public void DespawnPlayer()
        {
            if (State == PlayerState.Dead)
                return;

            State = PlayerState.Despawned;
        }

        public void SetWeaponLayer(int layer)
        {
            switch (layer)
            {
                case 1:
                    networkAnimator.Animator.SetLayerWeight(1, 1.0f);
                    networkAnimator.Animator.SetLayerWeight(2, 0.0f);
                    networkAnimator.Animator.SetLayerWeight(3, 0.0f);
                    break;
                case 2:
                    networkAnimator.Animator.SetLayerWeight(1, 0.0f);
                    networkAnimator.Animator.SetLayerWeight(2, 1.0f);
                    networkAnimator.Animator.SetLayerWeight(3, 0.0f);
                    break;
                case 3:
                    networkAnimator.Animator.SetLayerWeight(1, 0.0f);
                    networkAnimator.Animator.SetLayerWeight(2, 0.0f);
                    networkAnimator.Animator.SetLayerWeight(3, 1.0f);
                    break;
            }
        }

        public void SetActiveVirtualCamera(VirtualCameraType type)
        {
            if (Object.HasInputAuthority)
            {
                switch (type)
                {
                    case VirtualCameraType.FOLLOW:
                        {
                            CinemachineManager.Instance.thirdPersonCamera.Priority = 11;
                            CinemachineManager.Instance.spectatorCamera.Priority = 9;

                            _activeVirtualCameraType = VirtualCameraType.FOLLOW;

                            networkAnimator.Animator.SetInteger("LocomotionMode", 0);
                        }
                        break;

                    case VirtualCameraType.SPECTATOR:
                        {
                            CinemachineManager.Instance.thirdPersonCamera.Priority = 10;
                            CinemachineManager.Instance.spectatorCamera.Priority = 11;

                            _activeVirtualCameraType = VirtualCameraType.SPECTATOR;

                            networkAnimator.Animator.SetInteger("LocomotionMode", 0);
                        }
                        break;
                }
            }
        }

        // PRIVATE METHODS

        private void CheckRespawn()
        {
            _respawnInSeconds -= Runner.DeltaTime;
            if (_respawnInSeconds <= 0)
            {
                Debug.LogFormat("<color=green>Respawning player {0}, hasAuthority={1} from state={2}</color>", PlayerRefID, Object.HasStateAuthority, State);

                if (Object.HasInputAuthority)
                {
                    _pickupManager = FindObjectOfType<PickupManager>(true);
                }

                gameScore = 0;

                // Make sure we don't get in here again, even if we hit exactly zero
                _respawnInSeconds = -1;

                // Start the respawn timer and trigger the teleport in effect
                _respawnTimer = TickTimer.CreateFromSeconds(Runner, 1);

                // Place the player at its spawn point. This has to be done in FUN() because the transform gets reset otherwise
                if (KCC != null)
                {
                    Transform spawn = GameManager.Instance.levelManager.GetPlayerSpawnPoint().transform;

                    Vector3 positionDelta = spawn.position - this.transform.position;

                    // Must use the KCC to move the player.
                    KCC.SetPosition(spawn.position);
                    KCC.SetLookRotation(spawn.rotation, true);
                    KCC.SetDynamicVelocity(Vector3.zero);
                    KCC.SetKinematicVelocity(Vector3.zero);

                    // Update the camera on teleport.
                    CinemachineManager.Instance.thirdPersonCamera.OnTargetObjectWarped(CameraPivot, positionDelta);
                }

                // If the player was already here when we joined, it might already be active, in which case we don't want to trigger any spawn FX, so just leave it ACTIVE
                if (State != PlayerState.Active)
                    State = PlayerState.Spawning;

                Debug.LogFormat("Respawned player {0}, tick={1}, timer={2}:{3}, hasAuthority={4} to state={5}", PlayerRefID, Runner.Simulation.Tick, _respawnTimer.IsRunning, _respawnTimer.TargetTick, Object.HasStateAuthority, State);
            }
        }

        private void ResetPlayer()
        {
            Debug.Log($"Resetting player {PlayerRefID}, tick={Runner.Simulation.Tick}, timer={_respawnTimer.IsRunning}:{_respawnTimer.TargetTick}, hasAuthority={Object.HasStateAuthority} to state={State}");
            State = PlayerState.Active;

            // TODO:: This is the last place before the player is ready to go after spawing/respawning. For a cleaner experience we could unhide a screen to hide the popping into place and camera teleporting.
        }


        private bool HasLookRotationUpdateSource(ELookRotationUpdateSource source)
        {
            return (_lookRotationUpdateSource & source) == source;
        }

        private void CheckForPowerupPickup()
        {
            if (Object.HasInputAuthority)
            {
                if (_pickupManager == null)
                {
                    _pickupManager = FindObjectOfType<PickupManager>(true);
                }
            }

            // If we run into a wepon pickup, pick it up
            if (IsActivated && Runner.GetPhysicsScene().OverlapSphere(transform.position, _pickupRadius, _pickupOverlaps, _pickupMask, QueryTriggerInteraction.Collide) > 0)
            {
                WeaponSpawner weaponSpawner = _pickupOverlaps[0].GetComponent<WeaponSpawner>();
                float maxDist = _pickupRadius * 2.0f;

                // Get the closest weaponSpawner to us.
                foreach (Collider collider in _pickupOverlaps)
                {
                    WeaponSpawner spawner = collider.GetComponent<WeaponSpawner>();

                    float dist = Vector2.Distance(this.transform.position, spawner.transform.position);
                    if (dist < maxDist)
                    {
                        maxDist = dist;
                        weaponSpawner = spawner;
                    }
                }

                if (weaponSpawner == null)
                    return;

                // Here we can dispaly the pickup GUI.
                if (weaponSpawner.IsRespawning == false)
                {
                    if (_pickupManager != null)
                        _pickupManager.ShowPickupIndicator(weaponSpawner);
                }
                else
                {
                    if (_pickupManager != null)
                        _pickupManager.HidePickupIndicator(weaponSpawner);
                }

                // Run a timer while the action button is pressed.
                if (Input.FixedInput.Action == true && weaponSpawner.IsRespawning == false)
                {
                    if (!_pickUpActionTimer.IsRunning)
                    {
                        Debug.LogFormat("CheckForPowerupPickup - {0} - Timer Started.", Object.InputAuthority);

                        _pickUpActionTimer = TickTimer.CreateFromSeconds(Runner, _pickupActionTime);
                    }
                    else
                    {
                        // Use this to udpate the UI.

                        if (_pickupManager != null)
                        {
                            float percentage = (float)_pickUpActionTimer.RemainingTime(Runner) / _pickupActionTime;
                            _pickupManager.UpdateIndicatorRing(weaponSpawner, percentage);
                        }

                        if (_pickUpActionTimer.Expired(Runner))
                        {
                            Debug.LogFormat("CheckForPowerupPickup - {0} - Weapon Picked Up.", Object.InputAuthority);
                            PickupWeapon(_pickupOverlaps[0].GetComponent<WeaponSpawner>());

                            _pickUpActionTimer = TickTimer.None;
                        }
                    }
                }
                else
                {
                    // Clean up the UI.
                    if (_pickupManager != null)
                        _pickupManager.UpdateIndicatorRing(weaponSpawner, 0.0f);

                    // Clean up the timer.
                    if (_pickUpActionTimer.IsRunning)
                    {
                        _pickUpActionTimer = TickTimer.None;
                    }
                }
            }
            else
            {
                // Clean up the UI.
                if (_pickupManager != null)
                    _pickupManager.HidePickupIndicators();

                _pickupOverlaps.Clear();
            }
        }

        /// <summary>
		/// Called when a player collides with a weapon pickup.
		/// </summary>
		private void PickupWeapon(WeaponSpawner weaponSpawner)
        {
            if (!weaponSpawner || weaponSpawner.IsRespawning)
                return;

            WeaponElement weapon = weaponSpawner.Pickup();

            if (weapon == null)
                return;

            WeaponManager.PickUpWeapon(weapon);
        }
        
        public async void MarkFirstPlace()
        {
            if (!Object.HasInputAuthority) return;
            await BeamableStatsController.ChangeStat(FIRST_PLACE_KEY, "True");
        }

        public async void AddToMatchesWon()
        {
            if (!Object.HasInputAuthority) return;
            await BeamableStatsController.AddToStat(MATCHES_WON_KEY, 1);
        }

        public void SetNetworkAnimatorActive(bool active)
        {
            networkAnimator.Animator.enabled = active;
        }
        
        private async void TrackEvent(float time, int score)
        {
#if BEAMABLE_GAME_ANALYTICS
            var context = BeamContext.Default;
            var eventData = new GameResultsEvent(time, score, context.PlayerId.ToString());
            context.Api.AnalyticsTracker.TrackEvent(eventData, true);
#endif
        }

        // DATA STRUCTURES

        [Flags]
        private enum ELookRotationUpdateSource
        {
            Jump = 1 << 0, // Look rotation is updated on jump
            Movement = 1 << 1, // Look rotation is updated on character movement
            MouseMovement = 1 << 2, // Look rotation is updated on mouse move
        }

        public enum PlayerState
        {
            New,
            Despawned,
            Spawning,
            Active,
            Spectator,
            Safe,
            Disabled,
            Dead
        }
    }
}
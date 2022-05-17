using UnityEngine;
using Fusion;
using Fusion.KCC;
using BeamableExample.Helpers;

namespace BeamableExample.RedlightGreenLight.Character
{
	/// <summary>
	/// Base class for Simple and Advanced player implementations.
	/// Provides references to components and basic setup.
	/// </summary>
	[RequireComponent(typeof(KCC))]
	[OrderBefore(typeof(KCC))]
	[OrderAfter(typeof(NetworkCulling))]
	public abstract class BasePlayer : NetworkKCCProcessor, IBeforeUpdate
	{
		// PUBLIC MEMBERS

		public KCC				KCC     => _kcc;
		public PlayerInput		Input   => _input;
		public Camera			Camera => _camera;
		public GameObject		Visual  => _visual;
		public bool				IsLocal => _isLocal;
		public NetworkCulling	Culling => _culling;

		[Networked]
		public float SpeedMultiplier { get; set; } = 1.0f;

		// PROTECTED MEMBERS

		protected Transform  CameraPivot    => _cameraPivot;
		protected Transform GroupCameraPivot => _groupCameraPivot;
		protected float      MaxCameraAngle => _maxCameraAngle;
		protected Vector3    JumpImpulse    => _jumpImpulse;

		// PRIVATE MEMBERS

		[SerializeField]
		private GameObject _visual;
		[SerializeField]
		private Transform  _cameraPivot;
		[SerializeField]
		private Transform _groupCameraPivot;
		[SerializeField]
		private float      _maxCameraAngle;
		[SerializeField]
		private float      _areaOfInterestRadius;
		[SerializeField]
		private Vector3    _jumpImpulse;

		private KCC				_kcc;
		private PlayerInput		_input;
        private Camera			_camera;
        private bool			_isLocal;
		private NetworkCulling	_culling;

		// NetworkBehaviour INTERFACE

		public override void Spawned()
		{
			name = Object.InputAuthority.ToString();

			if (Object.HasInputAuthority == true)
			{
                _camera = Runner.SimulationUnityScene.GetComponent<Camera>();
                _isLocal = true;
			}

			// Explicit KCC initialization. This needs to be called before using API, otherwise changes could be overriden by implicit initialization from KCC.Start() or KCC.Spawned()
			_kcc.Initialize(EKCCDriver.Fusion);

			// Player itself can modify kinematic speed, registering to KCC
			_kcc.AddModifier(this);

			// Initialize input
			_input.SetPlayer(this);
		}

		public override void Despawned(NetworkRunner runner, bool hasState)
		{
			_input.SetPlayer(null);

            _camera = null;
            _isLocal = false;
		}

		public override void FixedUpdateNetwork()
		{
			// By default we expect derived classes to process input in FixedUpdateNetwork().
			// The correct approach is to set input before KCC updates internally => we need to specify [OrderBefore(typeof(KCC))] attribute.

			// SimplePlayer runs input processing in FixedUpdateNetwork() as expected, but KCC runs its internal update after Player.FixedUpdateNetwork().
			// Following call sets AoI position to last fixed update KCC position. It should not be a problem in most cases, but some one-frame glitches after teleporting might occur.
			// This problem is solved in AdvancedPlayer which uses manual KCC update at the cost of slightly increased complexity.

			Runner.AddPlayerAreaOfInterest(Object.InputAuthority, _kcc.FixedData.TargetPosition, _areaOfInterestRadius);
		}

		// IBeforeUpdate INTERFACE

		void IBeforeUpdate.BeforeUpdate()
		{
			if (_culling.IsCulled == true)
				return;

			_input.OnBeforeUpdate();
		}

		// NetworkKCCProcessor INTERFACE

		// Lowest priority => this processor will be executed last.
		public override float Priority => float.MinValue;

		public override EKCCStages GetValidStages(KCC kcc, KCCData data)
		{
			// Only SetKinematicSpeed stage is used, rest are filtered out and corresponding method calls will be skipped.
			return EKCCStages.SetKinematicSpeed;
		}

		public override void SetKinematicSpeed(KCC kcc, KCCData data)
		{
			// Applying multiplier.
			data.KinematicSpeed *= SpeedMultiplier;
		}

		// MonoBehaviour INTERFACE

		private void Awake()
		{
			_kcc     = gameObject.GetComponent<KCC>();
			_input   = gameObject.GetComponent<PlayerInput>();
			_culling = gameObject.GetComponent<NetworkCulling>();

			_culling.Updated = OnCullingUpdated;
		}

		// PRIVATE METHODS

		private void OnCullingUpdated(bool isCulled)
		{
			// Show/hide the game object based on AoI (Area of Interest)

			_visual.SetActive(isCulled == false);

			if (_kcc.Collider != null)
			{
				_kcc.Collider.gameObject.SetActive(isCulled == false);
			}
		}
	}
}

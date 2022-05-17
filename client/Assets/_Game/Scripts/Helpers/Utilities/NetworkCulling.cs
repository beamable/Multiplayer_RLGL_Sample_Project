using System;
using UnityEngine;
using Fusion;

namespace BeamableExample.Helpers
{
	/// <summary>
	/// Helper component for reliable culling notifications.
	/// </summary>
	[OrderBefore(typeof(NetworkTransformAnchor))]
	public sealed class NetworkCulling : NetworkBehaviour
	{
		// PUBLIC MEMBERS

		public Action<bool> Updated;

		public bool IsCulled => _isCulled;

		// PRIVATE MEMBERS

		[Networked]
		private NetworkBool _keepAlive { get; set; }

		private bool _isCulled;

		// NetworkBehaviour INTERFACE

		public override sealed void Spawned()
		{
			_isCulled = false;
		}

		public override sealed void Despawned(NetworkRunner runner, bool hasState)
		{
			_isCulled = false;
		}

		public override sealed void FixedUpdateNetwork()
		{
			if (Runner == null || Runner.IsForward == false)
				return;

			// Trigger network synchronization once per 30 ticks
			if (Runner.Simulation.Tick % 30 == 0)
			{
				_keepAlive = !_keepAlive;
			}

			// The object is treated as culled if it haven't received update for more than 60 ticks, probably out of AoI (Area of Interest)
			bool isCulled = Object.IsProxy == true && Object.LastReceiveTick > 0 && Object.LastReceiveTick < (Runner.Simulation.Tick - 60);

			if (_isCulled != isCulled)
			{
				_isCulled = isCulled;

				if (Updated != null)
				{
					try
					{
						Updated(isCulled);
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
					}
				}
			}
		}
	}
}

using UnityEngine;
using Fusion;
using BeamableExample.Extensions;

namespace BeamableExample.RedlightGreenLight.Character
{
	/// <summary>
	/// Input structure polled by Fusion. This is sent over network and processed by server, keep it optimized and remove unused data.
	/// </summary>
	public struct GameplayInput : INetworkInput
	{
		// PUBLIC MEMBERS

		public Vector2 MoveDirection;
		public Vector2 LookRotationDelta;
		public byte    Actions;

		public bool Sprint		{ get { return Actions.IsBitSet(0); } set { Actions.SetBit(0, value); } }
		public bool Jumping		{ get { return Actions.IsBitSet(1); } set { Actions.SetBit(1, value); } }
		public bool Attack		{ get { return Actions.IsBitSet(2); } set { Actions.SetBit(2, value); } }
		public bool Ready		{ get { return Actions.IsBitSet(3); } set { Actions.SetBit(3, value); } }
		public bool Jumped		{ get { return Actions.IsBitSet(4); } set { Actions.SetBit(4, value); } }
		public bool Action		{ get { return Actions.IsBitSet(5); } set { Actions.SetBit(5, value); } }
	}
}

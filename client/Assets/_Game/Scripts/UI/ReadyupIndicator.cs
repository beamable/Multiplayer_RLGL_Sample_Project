using BeamableExample.RedlightGreenLight.Character;
using UnityEngine;

namespace BeamableExample.RedlightGreenLight
{
	public class ReadyupIndicator : MonoBehaviour
	{
		[SerializeField] private CanvasGroup _canvasGroup;

		private Camera _camera;
		private Transform _target;

		public void Initialize(PlayerCharacter followPlayer)
		{
			_camera = Camera.main;
			_target = followPlayer.transform;
		}

		public void Show(bool value)
        {
			_canvasGroup.alpha = (value == true) ? 1.0f : 0.0f;
		}

		// Follow the assigned transform and scale up or down depending on if the player is ready or not
		private void LateUpdate()
		{
			if (_target == null)
				return;

			transform.position = _camera.WorldToScreenPoint(_target.position);
		}
	}
}
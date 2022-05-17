using Beamable.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeamableExample.RedlightGreenLight
{
	public class PickupIndicator : MonoBehaviour
	{
		[SerializeField] private UiViewComponent uiView;
		[SerializeField] private Image _ringImage;
		[SerializeField] private TextMeshProUGUI _hintText;
		[SerializeField] private TextMeshProUGUI _buttonHintText;
		[SerializeField] private Image _buttonHintImage;
		[SerializeField] private Image _weaponImage;
		[SerializeField] private bool trackWorldPosition = true;
		
		private bool isShown;
		private Camera _camera;
		private Transform _target;

		public void Initialize(WeaponSpawner weaponSpawner)
		{
			_camera = Camera.main;
			_target = weaponSpawner.transform;
			UpdateRing(0.0f);
			Show(false);

#if !UNITY_ANDROID && !UNITY_IOS
			PlayerInputListener.Instance.ControlTypeChangedAction += OnControlsTypeChanged;

			OnControlsTypeChanged(PlayerInputListener.Instance.currentControlScheme);
#endif
		}

        private void OnDestroy()
        {
			if (PlayerInputListener.Instance != null)
				PlayerInputListener.Instance.ControlTypeChangedAction -= OnControlsTypeChanged;
		}

		// TODO:: We need to make this work for specific controller types. ie PS Controllers and For Phones.
        private void OnControlsTypeChanged(string controlType)
        {
	        if (_hintText == null) return;
			switch (controlType)
			{
				case "Gamepad":
					{
						_hintText.text = "PRESS AND HOLD B TO PICK UP";

						if (_buttonHintImage != null)
							_buttonHintImage.gameObject.SetActive(true);

						if (_buttonHintText != null)
							_buttonHintText.text = "B";
					}
					break;

				case "Keyboard&Mouse":
					{
						_hintText.text = "PRESS AND HOLD E TO PICK UP";

						if (_buttonHintImage != null)
							_buttonHintImage.gameObject.SetActive(true);

						if (_buttonHintText != null)
							_buttonHintText.text = "E";
					}
					break;

				case "Touch":
                    {
                        _hintText.text = "PRESS AND HOLD TO PICK UP";

						if (_buttonHintImage != null)
							_buttonHintImage.gameObject.SetActive(false);

						if (_buttonHintText != null)
							_buttonHintText.text = "";
					}
					break;
				default:
					{
						_hintText.text = "PRESS AND HOLD E TO PICK UP";

						if (_buttonHintImage != null)
							_buttonHintImage.gameObject.SetActive(true);

						if (_buttonHintText != null)
							_buttonHintText.text = "E";
					}
					break;
			}
		}

        public void Show(bool value)
        {
	        isShown = value;
	        if (value)
	        {
		        uiView.Show();
		        return;
	        }
	        uiView.Hide();
		}

		// Follow the assigned transform and scale up or down depending on if the player is ready or not
		private void LateUpdate()
		{
			if (_target == null || !isShown)
				return;
			UpdateWorldPosition();
		}

		private void UpdateWorldPosition()
		{
			if (!trackWorldPosition) return;
			transform.position = _camera.WorldToScreenPoint(_target.position);
		}

		public void UpdateRing(float value)
        {
			_ringImage.fillAmount = value;
		}

		public void SetWeaponImage(Sprite weaponSprite)
        {
			_weaponImage.sprite = weaponSprite;
		}
	}
}
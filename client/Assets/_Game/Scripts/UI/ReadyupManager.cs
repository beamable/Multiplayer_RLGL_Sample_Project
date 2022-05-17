using BeamableExample.RedlightGreenLight.Character;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeamableExample.RedlightGreenLight
{
	public class ReadyupManager : NetworkBehaviour
	{
		[SerializeField] private bool _allowSoloPlay = false;
		[SerializeField] private TextMeshProUGUI _timerText;
		[SerializeField] private float _readyUpDelay = 30.0f;
		[SerializeField] private CanvasGroup _readyUpCanvasGroup;
		[SerializeField] private TextMeshProUGUI _readyCountText;
		[SerializeField] private GameObject _hintArea;
		[SerializeField] private TextMeshProUGUI _buttonHintText;

		private bool _allPlayersReady;

		[HideInInspector]
		[Networked]
		private TickTimer ReadyUpTimer { get; set; }

        public override void Spawned()
        {
			ReadyUpTimer = TickTimer.CreateFromSeconds(Runner, _readyUpDelay);

			var timeRemaining = ReadyUpTimer.RemainingTime(Runner);
			SetTimerText(timeRemaining.Value);

			_readyUpCanvasGroup.alpha = 1.0f;

#if !UNITY_ANDROID && !UNITY_IOS
			PlayerInputListener.Instance.ControlTypeChangedAction += OnControlsTypeChanged;

			OnControlsTypeChanged(PlayerInputListener.Instance.currentControlScheme);
#endif
		}

		private void OnControlsTypeChanged(string controlType)
		{
			switch (controlType)
			{
				case "Gamepad":
					{
						if (_hintArea != null)
							_hintArea.SetActive(true);

						if (_buttonHintText != null)
							_buttonHintText.text = "Y";
					}
					break;

				case "Keyboard&Mouse":
					{
						if (_hintArea != null)
							_hintArea.SetActive(true);

						if (_buttonHintText != null)
							_buttonHintText.text = "R";
					}
					break;

				case "Touch":
					{
						if (_hintArea != null)
							_hintArea.SetActive(false);

						if (_buttonHintText != null)
							_buttonHintText.text = "";
					}
					break;
				default:
					{
						if (_hintArea != null)
							_hintArea.SetActive(true);

						if (_buttonHintText != null)
							_buttonHintText.text = "R";
					}
					break;
			}
		}

		private void OnDestroy()
		{
			if (PlayerInputListener.Instance != null)
				PlayerInputListener.Instance.ControlTypeChangedAction -= OnControlsTypeChanged;
		}

		public override void FixedUpdateNetwork()
        {
			if (ReadyUpTimer.IsRunning)
			{
				var timeRemaining = ReadyUpTimer.RemainingTime(Runner);
				SetTimerText(timeRemaining.Value);

				if (Object.HasStateAuthority)
				{
					if (ReadyUpTimer.Expired(Runner))
					{
						ReadyUpTimer = TickTimer.None;

						Debug.Log("<color=green>ReadyUpTimer Expired</color>");
						foreach (PlayerCharacter player in PlayerManager.allPlayers)
						{
							player.SetReady(true);
						}
					}
				}
			}
		}

		public override void Render()
        {
			if (_allPlayersReady || GameManager.playState == GameManager.GameState.LEVEL)
				return;

			// Are all players ready?
			_allPlayersReady = PlayerManager.allPlayers.Count > 1 || (PlayerManager.allPlayers.Count == 1 && _allowSoloPlay);

			int readyCount = 0;
			foreach (PlayerCharacter player in PlayerManager.allPlayers)
			{
				// Update the ready indicator on the player.
				player.playerInfoUI.ShowReady(player.IsReady());

				// If a player is not ready than lets remember that.
				if (!player.IsReady())
					_allPlayersReady = false;
				else
					readyCount++;
			}

			// Update ready player text.
			_readyCountText.text = string.Format("{0}/{1}", readyCount, PlayerManager.allPlayers.Count);

			if (_allPlayersReady)
			{
				GameManager.Instance.OnAllPlayersReady();

				if (ReadyUpTimer.IsRunning)
					ReadyUpTimer = TickTimer.None;
			}
		}

		public void HideReadyUpUI()
		{
			_readyUpCanvasGroup.alpha = 0.0f;
			ResetReadyIndicators();
		}

		private void ResetReadyIndicators()
		{
			foreach (PlayerCharacter player in PlayerManager.allPlayers)
			{
				player.playerInfoUI.ShowReady(false);
			}
		}

		private void SetTimerText(float time)
		{
			var minutes = Mathf.FloorToInt(time / 60);
			var seconds = Mathf.FloorToInt(time % 60);
			_timerText.text = minutes.ToString("00") + ":" + seconds.ToString("00");
		}
	}
}
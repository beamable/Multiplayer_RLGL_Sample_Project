using Beamable.UI;
using BeamableExample.RedlightGreenLight;
using BeamableExample.RedlightGreenLight.Character;
using TMPro;
using UnityEngine;

namespace BeamableExample.RedlightGreenLight
{
    public class EliminationUI : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private TextMeshProUGUI eliminationTimeText;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI eliminationReasonText;

        public void Initialize(float time, int score, string reason, bool show)
        {
            SetTimerText(time);
            SetScoreText(score);
            SetEliminationReasonText(reason);
            Show(show);
        }

        private void SetTimerText(float time)
        {
            var minutes = Mathf.FloorToInt(time / 60);
            var seconds = Mathf.FloorToInt(time % 60);
            eliminationTimeText.text = minutes.ToString("00") + ":" + seconds.ToString("00");
        }

        private void SetScoreText(int score)
        {
            scoreText.text = score.ToString();
        }

        private void SetEliminationReasonText(string reason)
        {
            eliminationReasonText.text = reason;
        }

        public void Show(bool value)
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
            
            if (value)
            {
                UiStateController.Show("eliminated", "game");
            }
            else
            {
                UiStateController.Hide("eliminated", "game");
            }
            
        }

        public void OnSpectatorSelected()
        {
            // TODO:: Enter spectator mode for this player.
            PlayerCharacter player = PlayerManager.GetPlayerWithInputAuthority();
            if (player != null)
            {
                Show(false);
                player.EnterSpectatorMode();
                
            }
        }

        public void OnQuitSelected()
        {
            // End the game and go back to the main menu.
            GameManager.Instance.Restart(Fusion.ShutdownReason.Ok);
        }
    }
}

using System;
using System.Collections.Generic;
using Beamable.Microservices;
using Beamable.UI;
using BeamableExample.RedlightGreenLight;
using BeamableExample.RedlightGreenLight.Character;
using ListView;
using TMPro;
using UnityEngine;

namespace BeamableExample.RedlightGreenLight
{
    public class MatchCompletedUI : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private TextMeshProUGUI eliminationTimeText;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private ListViewComponent _ListView;
        [SerializeField] private TextMeshProUGUI eliminationReasonText;

        private ListViewData _listViewData = new ListViewData();
        private PostGameResult _postGameResult = new PostGameResult();
        
        public void Initialize(PostGameResult postGameResult, float time, int score, string reason, bool show)
        {
            _postGameResult = postGameResult;
            SetTimerText(time);
            SetScoreText(score);
            SetEliminationReasonText(reason);

            if (postGameResult.Rewards != null && postGameResult.Rewards.Count > 0)
            {
                foreach (var reward in postGameResult.Rewards)
                {
                    _listViewData.Add(new ListItem()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Title = reward.IsItem ? reward.ItemContentId : reward.CurrencyType,
                        PropertyBag = new Dictionary<string, object>()
                        {
                            {"reward", reward}
                        }
                    });
                }
                _ListView.Build(_listViewData);
            }
            
            if (!show) return;
            UiStateController.Show("MatchComplete","game");
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
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

        public void OnSpectatorSelected()
        {
            // TODO:: Enter spectator mode for this player.
            var player = PlayerManager.GetPlayerWithInputAuthority();
            if (player == null) return;
            player.EnterSpectatorMode();
            UiStateController.Hide("MatchComplete","game");
        }

    }
}

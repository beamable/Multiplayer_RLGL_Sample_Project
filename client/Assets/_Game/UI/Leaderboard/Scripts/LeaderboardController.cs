using System;
using System.Collections.Generic;
using Beamable;
using Beamable.Common.Api.Leaderboards;
using Beamable.UI;
using BeamableExample.RedlightGreenLight;
using ListView;
using UnityEngine;

public class LeaderboardController : MonoBehaviour
{
    [SerializeField] private ListViewComponent listViewComponent;
    [SerializeField] private ScoreCard footerCard;
    private IBeamableAPI _beamableAPI;
    private LeaderBoardView _leaderBoardView;

    private string _leaderboardId = "leaderboards.ranked";

   
    private void Awake()
    {
        SetUpBeamable();
    }

    private async void SetUpBeamable()
    {
        _beamableAPI = await Beamable.API.Instance;
    }

    public async void GetLeaderboard()
    {
        _leaderBoardView = await _beamableAPI.LeaderboardService.GetBoard(_leaderboardId, 0, 100, _beamableAPI.User.id);
        UpdateList();
    }
    
    public async void UpdateList()
    {
        var cardData = new ListViewData();

        foreach (var rankEntry in _leaderBoardView.rankings)
        {
            var stats = await _beamableAPI.StatsService.GetStats("client", "public", "player", rankEntry.gt);
            var alias = stats.ContainsKey("alias") ? stats["alias"] : "Anonymous";
            var entryStats = rankEntry.stats;
            cardData.Add(new ListItem
            {
                Title = alias,
                PropertyBag = new Dictionary<string, object>()
                {
                    {"rank", rankEntry.rank},
                    {"score", rankEntry.score},
                    {"current_player", rankEntry.gt == BeamContext.Default.PlayerId}
                },
                ListPrefabIndex = 0
            });
            if (rankEntry.gt == BeamContext.Default.PlayerId)
            {
                footerCard.SetUp(cardData[cardData.Count-1]);
            }
        }

        listViewComponent.Build(cardData);
    }

    public void OnQuitSelected()
    { 
        UiStateController.Show("loading", "game");
        GameManager.Instance.Restart(Fusion.ShutdownReason.Ok);
    }
}

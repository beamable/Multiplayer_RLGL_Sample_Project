using System.Collections.Generic;
using System.Linq;
using Beamable;
using Beamable.Common.Content;
using Beamable.Experimental.Api.Matchmaking;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class MatchmakingHandler : MonoBehaviour
{
    [SerializeField] private SimGameTypeRef gameTypeRef;
    [SerializeField] private TextMeshProUGUI handleOutput;

    [SerializeField] private UnityEvent OnSearchCancelled;
    [SerializeField] private UnityEvent OnSearchTimeout;
    [SerializeField] private UnityEvent<string> OnMatchStart;
    [SerializeField] private UnityEvent<string> OnMatchReady;
    [SerializeField] private UnityEvent<int> OnUpdatePlayers;
    
    private string _currentTicketId;

    private IBeamableAPI _beamableAPI;

    private List<string> _players = new List<string>();

    private float _startTime;

    private async void Start()
    {
        _beamableAPI = await API.Instance;
        Debug.Log($"User Id: {_beamableAPI.User.id}");
        LogToOutput("API Initialized.");
    }

    public async void StartMatchmaking()
    {
        OnMatchStart?.Invoke("NONE");
        var gameType = await gameTypeRef.Resolve();

        var handle =  await _beamableAPI.Experimental.MatchmakingService
            .StartMatchmaking(gameTypeRef, UpdateHandler, ReadyHandler, TimeoutHandler).Error(error =>
            {
                LogToOutput("Matchmaking failed to start search!");
                LogToOutput(error.Message);
                LogToOutput(error.StackTrace);
            });

           _currentTicketId = handle.Tickets.First().ticketId;
           LogToOutput("Started Matchmaking search!");
           _startTime = Time.time;
    }

    public async void CancelMatchmaking()
    {
        LogToOutput($"Attempting to cancel ticket ID: {_currentTicketId}");
        await _beamableAPI.Experimental.MatchmakingService.CancelMatchmaking(_currentTicketId).Then(_ =>
        {
            LogToOutput("Cancelled Successfully.");
            OnSearchCancelled?.Invoke();
        }).Error(error =>
        {
            LogToOutput("Cancellation Failed!");
            LogToOutput(error.Message);
            LogToOutput(error.StackTrace);
            OnSearchCancelled?.Invoke();
        });
    }

    private void TimeoutHandler(MatchmakingHandle handle)
    {
        LogToOutput(handle.State.ToString());
        OnSearchTimeout?.Invoke();
    }

    private void ReadyHandler(MatchmakingHandle handle)
    {
        LogToOutput(handle.State.ToString());
        LogToOutput(JsonUtility.ToJson(handle.Match));
        TrackEvent(Time.time - _startTime, handle.Match.matchId);
        OnMatchReady?.Invoke(handle.Match.matchId);
    }

    private void UpdateHandler(MatchmakingHandle handle)
    {
        LogToOutput(handle.State.ToString());
        var teams = handle.Tickets.Where(ticket => ticket.players != null);
        
        foreach (var team in teams)
        {
            _players.AddRange(team.players.ToList());
        }

        OnUpdatePlayers?.Invoke(_players.Count);
    }

    private void LogToOutput(string log)
    {
        handleOutput.text += $"\n{log}";
    }
    
    private void TrackEvent(float time, string matchId)
    {
#if BEAMABLE_MATCHMAKING_ANALYTICS
        var eventData = new MatchmakingTimeEvent(time, _currentTicketId, matchId, _beamableAPI.User.id.ToString());
        _beamableAPI.AnalyticsTracker.TrackEvent(eventData, true);
#endif
    }
}
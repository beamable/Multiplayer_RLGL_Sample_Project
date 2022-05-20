using System.Collections.Generic;
using System.Linq;
using Beamable;
using Beamable.Common.Content;
using Beamable.Experimental.Api.Matchmaking;
using UnityEngine;
using UnityEngine.Events;

public class MatchmakingHandler : MonoBehaviour
{
    [SerializeField] private SimGameTypeRef gameTypeRef;

    [SerializeField] private UnityEvent OnSearchCancelled;
    [SerializeField] private UnityEvent OnSearchTimeout;
    [SerializeField] private UnityEvent<string> OnMatchStart;
    [SerializeField] private UnityEvent<string> OnMatchReady;
    [SerializeField] private UnityEvent<int> OnUpdatePlayers;
    
    private string _currentTicketId;

    private BeamContext _context;

    private List<string> _players = new List<string>();

    private float _startTime;

    private async void Start()
    {
        _context = BeamContext.Default;
        await _context.OnReady;
        Debug.Log($"User Id: {_context.PlayerId}");
        LogToOutput("API Initialized.");
    }

    public async void StartMatchmaking()
    {
        OnMatchStart?.Invoke("NONE");
        var gameType = await gameTypeRef.Resolve();

        var handle =  await _context.Api.Experimental.MatchmakingService
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
        await _context.Api.Experimental.MatchmakingService.CancelMatchmaking(_currentTicketId).Then(_ =>
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
        Debug.Log($"\n{log}");
    }
    
    private void TrackEvent(float time, string matchId)
    {
#if BEAMABLE_MATCHMAKING_ANALYTICS
        var eventData = new MatchmakingTimeEvent(time, _currentTicketId, matchId, _context.PlayerId.ToString());
        _context.Api.AnalyticsTracker.TrackEvent(eventData, true);
#endif
    }
}
using System.Collections;
using System.Collections.Generic;
using Beamable.Api.Analytics;
using UnityEngine;

public class MatchmakingTimeEvent : CoreEvent
{
    public MatchmakingTimeEvent(float timeElapsed, string ticketId, string matchId, string dbid) : base(
        "matchmaking_event",
        "matchmaking_time",
        new Dictionary<string, object>()
        {
            ["timeElapsed"] = timeElapsed,
            ["ticketId"] = ticketId,
            ["matchId"] = matchId,
            ["dbid"] = dbid
        }
    ) { }
}

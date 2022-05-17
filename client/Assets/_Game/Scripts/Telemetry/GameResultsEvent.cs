using System.Collections;
using System.Collections.Generic;
using Beamable.Api.Analytics;
using UnityEngine;

public class GameResultsEvent : CoreEvent
{
    public GameResultsEvent(float timeElapsed, int score, string dbid) : base(
        "game_event",
        "game_results",
        new Dictionary<string, object>()
        {
            ["timeElapsed"] = timeElapsed,
            ["score"] = score,
            ["dbid"] = dbid
        }
    ) { }
}

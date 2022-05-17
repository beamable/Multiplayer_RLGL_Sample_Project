# Telemetry

### In-Game Usages

- Used when matchmaking completes to report the amount of time it took a player to get into a match.
- Used when a match ends to report the match's results.

Preprocessor directives are used around the calls to the Telemetry service. These are specific to a feature, and can be used to disable telemetry data when desired, reducing the number of API calls:

```csharp
#if BEAMABLE_MATCHMAKING_ANALYTICS
        var eventData = new MatchmakingTimeEvent(time, _currentTicketId, matchId, _beamableAPI.User.id.ToString());
        _beamableAPI.AnalyticsTracker.TrackEvent(eventData, true);
#endif
```

### Class Breakdown

The telemetry structures used in the project both inherit from Beamable's CoreEvent class.

**MatchmakingTimeEvent** - Contains the time it took for a player to get into a match.
- Information reported includes the time elapsed for getting into a match, the matchmaking ticket ID, the ID of the match itself, and the player's ID.
- This is written from the MatchmakingHandler script once matchmaking completes to grab information about the matchmaking session.

**GameResultsEvent** - Contains the player's score and the time of a match when a match completes.
- Information reported are the time elapsed for the match, the player's score for the match, and the player's ID.
- This is written from the PlayerCharacter script once a player completes a match to grab the information about that match.

> To learn more about basic usage of the Telemetry feature, read more on Beamable's documentation site [here](https://docs.beamable.com/docs/telemetry-feature-overview).

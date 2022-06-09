using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beamable;
using Beamable.Api.Payments;
using Beamable.Server.Clients;
using UnityEngine;

public static class BeamableStatsController
{
    private static BeamContext _beamContext;
    private static AchievementsServiceClient _achievementsService;
    private static Dictionary<string, string> _stats;
    private static List<string> currentEarnedAchievements = new List<string>();

    private const string ACCESS = "public";
    private const string DOMAIN = "client";
    private const string STAT_TYPE = "player";

    public static async Task SetUpBeamable()
    {
        _beamContext = BeamContext.Default;
        _stats = await _beamContext.Api.StatsService.GetStats(DOMAIN, ACCESS, STAT_TYPE,_beamContext.PlayerId);
        _achievementsService = new AchievementsServiceClient();
    }
    
    public static async Task AddToStat(string key, int amount)
    {
        await SetUpBeamable();
        _stats = await _beamContext.Api.StatsService.GetStats(DOMAIN, ACCESS, STAT_TYPE, _beamContext.PlayerId);
        _stats.TryGetValue(key, out var value);
        value ??= "0";
        var newAmount = int.Parse(value) + amount;
        var newStats = new Dictionary<string, string>
        {
            {key, newAmount.ToString()}
        };
        await _beamContext.Api.StatsService.SetStats(ACCESS, newStats);

        NewAchievementsEarned(key);
    }
    
    public static async Task ChangeStat(string key, string value)
    {
        await SetUpBeamable();
        _stats = await _beamContext.Api.StatsService.GetStats(DOMAIN, ACCESS, STAT_TYPE, _beamContext.PlayerId);
        var newStats = new Dictionary<string, string>
        {
            {key, value}
        };
        await _beamContext.Api.StatsService.SetStats(ACCESS, newStats);

        NewAchievementsEarned(key);
    }
    
    public static async Task<bool> CheckIfHasStat(string key)
    {
        await SetUpBeamable();
        _stats = await _beamContext.Api.StatsService.GetStats(DOMAIN, ACCESS, STAT_TYPE, _beamContext.PlayerId);
        return _stats.ContainsKey(key);
    }

    public static async void NewAchievementsEarned(string key)
    {
        await SetUpBeamable();
        var completedAchievements = await _achievementsService.CheckAchievements(key);
        var newAchievementsEarned = new List<string>();
        foreach (var achievement in completedAchievements)
        {
            if (!currentEarnedAchievements.Contains(achievement))
            {
                newAchievementsEarned.Add(achievement);
            }
        }
        foreach (var achievement in newAchievementsEarned)
        {
            await _achievementsService.AchievementEarnedNotification(_beamContext.PlayerId,
                achievement);
        }
    }

    public static void SetCurrentEarnedAchievements(List<string> earnedAchievements)
    {
        currentEarnedAchievements = earnedAchievements;
    }
}

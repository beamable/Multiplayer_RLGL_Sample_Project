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
    private static IBeamableAPI _beamableAPI;
    private static AchievementsServiceClient _achievementsService;
    private static Dictionary<string, string> _stats;
    private static List<string> currentEarnedAchievements = new List<string>();

    private const string ACCESS = "public";
    private const string DOMAIN = "client";
    private const string STAT_TYPE = "player";

    public static async Task SetUpBeamable()
    {
        _beamableAPI = await Beamable.API.Instance;
        _stats = await _beamableAPI.StatsService.GetStats(DOMAIN, ACCESS, STAT_TYPE, _beamableAPI.User.id);
        _achievementsService = new AchievementsServiceClient();
    }
    
    public static async Task AddToStat(string key, int amount)
    {
        await SetUpBeamable();
        _stats = await _beamableAPI.StatsService.GetStats(DOMAIN, ACCESS, STAT_TYPE, _beamableAPI.User.id);
        _stats.TryGetValue(key, out var value);
        value ??= "0";
        var newAmount = int.Parse(value) + amount;
        var newStats = new Dictionary<string, string>
        {
            {key, newAmount.ToString()}
        };
        await _beamableAPI.StatsService.SetStats(ACCESS, newStats);

        NewAchievementsEarned(key);
    }
    
    public static async Task ChangeStat(string key, string value)
    {
        await SetUpBeamable();
        _stats = await _beamableAPI.StatsService.GetStats(DOMAIN, ACCESS, STAT_TYPE, _beamableAPI.User.id);
        var newStats = new Dictionary<string, string>
        {
            {key, value}
        };
        await _beamableAPI.StatsService.SetStats(ACCESS, newStats);

        NewAchievementsEarned(key);
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
            await _achievementsService.AchievementEarnedNotification(_beamableAPI.User.id,
                achievement);
        }
    }

    public static void SetCurrentEarnedAchievements(List<string> earnedAchievements)
    {
        currentEarnedAchievements = earnedAchievements;
    }
}

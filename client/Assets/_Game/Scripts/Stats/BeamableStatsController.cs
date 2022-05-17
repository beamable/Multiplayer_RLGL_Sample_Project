using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beamable;
using UnityEngine;

public class BeamableStatsController : MonoBehaviour
{
    private IBeamableAPI _beamableAPI;
    private Dictionary<string, string> _stats;
    
    private const string ACCESS = "public";
    private const string DOMAIN = "client";
    private const string STAT_TYPE = "player";

    private void OnEnable()
    {
        SetUpBeamable();
    }

    private async void SetUpBeamable()
    {
        _beamableAPI = await Beamable.API.Instance;
        _stats = await _beamableAPI.StatsService.GetStats(DOMAIN, ACCESS, STAT_TYPE, _beamableAPI.User.id);
    }
    
    public async Task AddToStat(string key, int amount)
    {
        _stats = await _beamableAPI.StatsService.GetStats(DOMAIN, ACCESS, STAT_TYPE, _beamableAPI.User.id);
        _stats.TryGetValue(key, out var value);
        value ??= "0";
        var newAmount = int.Parse(value) + amount;
        var newStats = new Dictionary<string, string>
        {
            {key, newAmount.ToString()}
        };
        await _beamableAPI.StatsService.SetStats(ACCESS, newStats);
    }
    
    public async Task ChangeStat(string key, string value)
    {
        _stats = await _beamableAPI.StatsService.GetStats(DOMAIN, ACCESS, STAT_TYPE, _beamableAPI.User.id);
        var newStats = new Dictionary<string, string>
        {
            {key, value}
        };
        await _beamableAPI.StatsService.SetStats(ACCESS, newStats);
    }
}

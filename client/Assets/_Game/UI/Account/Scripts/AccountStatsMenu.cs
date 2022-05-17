using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beamable;
using ListView;
using UnityEngine;

public class AccountStatsMenu : MonoBehaviour
{
    [Serializable]
    public struct NamedStat
    {
        public string statKey;
        public string name;
    }
    [SerializeField] private List<NamedStat> namedStats;
    [SerializeField] private ListViewComponent statsListView;
    
    private IBeamableAPI _beamableAPI;
    private long _userId;
    private Dictionary<string, string> _stats;

    private async void OnEnable()
    {
        await SetUpBeamable();
        UpdateStatsList();
    }

    private async Task SetUpBeamable()
    {
        _beamableAPI = await API.Instance;
        _userId = _beamableAPI.User.id;
        await GetAllStats();
    }

    private async Task GetAllStats()
    {
        await _beamableAPI.StatsService.SetStats("public", new Dictionary<string, string>());
        _stats = await _beamableAPI.StatsService.GetStats("client", "public", "player", _userId);
    }
    
    public async void UpdateStatsList()
    {
        await GetAllStats();
        var cardData = new ListViewData();
        foreach (var stat in namedStats)
        {
            cardData.Add(new ListItem
            {
                Title = stat.name,
                PropertyBag = new Dictionary<string, object>()
                {
                    {"amount", GetPlayerStat(stat.statKey) ?? "0"}
                }
            });
        }
        statsListView.Build(cardData);
    }
    
    private string GetPlayerStat(string statKey)
    {
        _stats.TryGetValue(statKey, out var value);
        return value;
    }
}

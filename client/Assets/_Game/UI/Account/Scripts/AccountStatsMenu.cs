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

    private BeamContext _context;
    private long _userId;
    private Dictionary<string, string> _stats;

    private async void OnEnable()
    {
        await SetUpBeamable();
        UpdateStatsList();
    }

    private async Task SetUpBeamable()
    {
        _context = BeamContext.Default;
        await _context.OnReady;
        _userId = _context.PlayerId;
        await GetAllStats();
    }

    private async Task GetAllStats()
    {
        await _context.Api.StatsService.SetStats("public", new Dictionary<string, string>());
        _stats = await _context.Api.StatsService.GetStats("client", "public", "player", _userId);
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

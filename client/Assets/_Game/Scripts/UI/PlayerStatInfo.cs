using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beamable;
using Beamable.Avatars;
using Beamable.Stats;
using ListView;
using UnityEngine;
using UnityEngine.Events;

public class PlayerStatInfo : MonoBehaviour
{
    private IBeamableAPI _beamableAPI;
    private long _userId;
    private Dictionary<string, string> _stats;

    [SerializeField] private StatObject aliasStat;
    [SerializeField] private StatObject avatarStat;
    [SerializeField] private StatObject gamerTagStat;
    
    public UnityEvent<string> OnGetUsername;
    public UnityEvent<Sprite> OnGetIcon;
    public UnityEvent<string> OnGetIconName;
    
    private void OnEnable()
    {
        SetUpBeamable();
    }

    private async void SetUpBeamable()
    {
        _beamableAPI = await API.Instance;
        _userId = _beamableAPI.User.id;
        _stats = await _beamableAPI.StatsService.GetStats("client", "public", "player", _userId);
        GetAllStats();
    }

    public void GetAllStats()
    {
        GetUsername();
        GetIcon();
    }
    
    private string GetPlayerStat(string statKey)
    {
        _stats.TryGetValue(statKey, out var value);
        return value;
    }
    

    public void GetUsername()
    {
        OnGetUsername?.Invoke(GetPlayerStat(aliasStat.StatKey) ?? aliasStat.DefaultValue);
    }

    public void GetIcon()
    {
        var spriteId = GetPlayerStat(avatarStat.StatKey)  ?? avatarStat.DefaultValue;

        OnGetIcon?.Invoke(!string.IsNullOrWhiteSpace(spriteId)
            ? GetAvatar(spriteId)
            : AvatarConfiguration.Instance.Default.Sprite);
    }
    
    private Sprite GetAvatar(string id)
    {
        List<AccountAvatar> accountAvatars = AvatarConfiguration.Instance.Avatars;
        AccountAvatar accountAvatar = accountAvatars.Find(avatar => avatar.Name == id);
        return accountAvatar.Sprite;
    }

    public void GetIconName()
    {
        OnGetIconName?.Invoke(GetPlayerStat(avatarStat.StatKey) ?? avatarStat.DefaultValue);
    }

    private async Task SetPlayerStat(string statKey, string value,string domainOverride = "client")
    {
        Dictionary<string, string> setStats = new Dictionary<string, string>(){{statKey, value}};
        await _beamableAPI.StatsService.SetStats("public", setStats);
        _stats = await _beamableAPI.StatsService.GetStats(domainOverride, "public", "player", _userId);
    }

    public async void SetUsername(string alias)
    {
        await SetPlayerStat(aliasStat.StatKey, alias);
        GetUsername();
    }

    public async void SetIcon(string value)
    {
        await SetPlayerStat(avatarStat.StatKey, value);
        GetIcon();
    }
}

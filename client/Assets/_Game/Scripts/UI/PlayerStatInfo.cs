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
    private BeamContext _context;
    private long _userId;
    private Dictionary<string, string> _stats;

    [SerializeField] private StatObject aliasStat;
    [SerializeField] private StatObject avatarStat;
    [SerializeField] private StatObject gamerTagStat;
    
    public UnityEvent<string> OnGetUsername;
    public UnityEvent<Sprite> OnGetIcon;
    public UnityEvent<string> OnGetIconName;
    public UnityEvent<string> OnGetEmail;
    public UnityEvent<bool> OnHasNoEmail;

    private void OnEnable()
    {
        SetUpBeamable();
    }

    private async void SetUpBeamable()
    {
        _context = BeamContext.Default;
        await _context.OnReady;
        _userId = _context.PlayerId;
        _stats = await _context.Api.StatsService.GetStats("client", "public", "player", _userId);
        GetAllStats();
        GetEmail();
    }

    public void GetAllStats()
    {
        GetUsername();
        GetIcon();
    }
    
    private async Task<string> GetPlayerStat(string statKey)
    {
        var blankStats = new Dictionary<string, string>();
        await _context.Api.StatsService.SetStats("public", blankStats);
        _stats.TryGetValue(statKey, out var value);
        return value;
    }
    

    public async void GetUsername()
    {
        OnGetUsername?.Invoke(await GetPlayerStat(aliasStat.StatKey) ?? aliasStat.DefaultValue);
    }

    public async void GetIcon()
    {
        var spriteId = await GetPlayerStat(avatarStat.StatKey)  ?? avatarStat.DefaultValue;

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

    public async void GetIconName()
    {
        OnGetIconName?.Invoke(await GetPlayerStat(avatarStat.StatKey) ?? avatarStat.DefaultValue);
    }

    private async Task SetPlayerStat(string statKey, string value,string domainOverride = "client")
    {
        Dictionary<string, string> setStats = new Dictionary<string, string>(){{statKey, value}};
        await _context.Api.StatsService.SetStats("public", setStats);
        _stats = await _context.Api.StatsService.GetStats(domainOverride, "public", "player", _userId);
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

    public void GetEmail()
    {
        OnHasNoEmail?.Invoke(!_context.Api.User.HasDBCredentials());
        OnGetEmail?.Invoke(_context.Api.User.HasDBCredentials() ? _context.Api.User.email : "");
    }
}

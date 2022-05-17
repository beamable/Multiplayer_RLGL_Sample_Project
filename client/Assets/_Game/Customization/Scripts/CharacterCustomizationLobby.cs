using System.Linq;
using System.Threading.Tasks;
using _Game.Features.Customization.Scripts;
using JetBrains.Annotations;
using UnityEngine;
using Beamable;
using System.Collections.Generic;

public class CharacterCustomizationLobby : MonoBehaviour
{
    [SerializeField] private ColorPalette palette;
    public CharacterCustomizationAsset[] hairAssets;
    public CharacterCustomizationAsset[] bodyAssets;

    [SerializeField]
    private List<PlayerCustomizationCategory> categories = new List<PlayerCustomizationCategory>();

    private const string DOMAIN = "client";
    private const string ACCESS = "public";
    private const string TYPE = "player";
    private IBeamableAPI _beamableAPI;
    private long _userId;
    private Dictionary<string, string> _stats = new Dictionary<string, string>();
    private Dictionary<string, string> _tempStats = new Dictionary<string, string>();

    private void OnEnable()
    {
        // fill out a temp stat dic so we can use it on preview changes later.
        foreach (var category in categories)
        {
            var stat = category.categoryStat;
            var doesStatExist = _stats.TryGetValue(stat.StatKey, out var value);
            var statValue = doesStatExist ? value : stat.DefaultValue;
            _tempStats.Add(stat.StatKey, statValue);
        }

        SetUpBeamable();
    }

    private async void SetUpBeamable()
    {
        _beamableAPI = await API.Instance;
        _userId = _beamableAPI.User.id;
        await GetAllStats();
        GetAllStatCategories();

        SetHairFromStat();
        SetHairColorFromStat();
        SetBodyColorFromStat();
    }

    public async Task GetAllStats()
    {
        _stats = await _beamableAPI.StatsService.GetStats(DOMAIN, ACCESS, TYPE, _userId);
    }

    public void GetAllStatCategories()
    {
        foreach (var category in categories)
        {
            GetStat(category);
        }
    }

    private string GetStat(PlayerCustomizationCategory category)
    {
        if (_stats.Count == 0) return "";
        var stat = category.categoryStat;
        var doesStatExist = _stats.TryGetValue(stat.StatKey, out var value);
        var statValue = doesStatExist ? value : stat.DefaultValue;
        category.OnStatChanged?.Invoke(statValue);
        return statValue;
    }

    public async Task<string> GetStatById(string id)
    {
        if (_stats.Count == 0)
        {
            await GetAllStats();
        }

        var category = categories.Find(cat => cat.id == id);
        return GetStat(category);
    }

    public void SetAllStats()
    {
        foreach (KeyValuePair<string, string> stats in _tempStats)
        {
            SetStat(stats.Key, stats.Value);
        }
    }

    public async void SetStat(string statKey, string value)
    {
        var category = categories.Find(cat => cat.categoryStat.StatKey == statKey);
        if (category == null)
        {
            Debug.LogError($"Could not find customization category {statKey}");
            return;
        }
        await SetPlayerStat(statKey, value);
        GetStat(category);
    }

    [UsedImplicitly]
    public void PreviewStatChange(string statKey, string value)
    {
        var category = categories.Find(cat => cat.categoryStat.StatKey == statKey);
        if (category == null)
        {
            Debug.LogError($"Could not find customization category {statKey}");
            return;
        }
        if (_tempStats.ContainsKey(statKey))
            _tempStats[statKey] = value;
        else
            _tempStats.Add(statKey, value);

        category.OnStatChanged?.Invoke(value);
    }

    private async Task SetPlayerStat(string statKey, string value)
    {
        Dictionary<string, string> setStats = new Dictionary<string, string>() { { statKey, value } };
        await _beamableAPI.StatsService.SetStats(ACCESS, setStats);
        await GetAllStats();
    }

    private async void SetHairFromStat()
    {
        var hairStyle = await GetStatById("hairStyle");
        var hairIndex = hairAssets.ToList().FindIndex(hair => hair.beamableID == hairStyle);
        SetHair(hairIndex);
    }

    public async void SetHairColorFromStat()
    {
        var hairColor = await GetStatById("hairColor");
        SetHairColorById(hairColor);
    }
    
    public async void SetBodyColorFromStat()
    {
        var bodyColor = await GetStatById("bodyColor");
        SetBodyColorById(bodyColor);
    }

    public void Initialize()
    {
        foreach (CharacterCustomizationAsset asset in hairAssets)
        {
            asset.joints = asset.renderer.GetComponentsInChildren<ConfigurableJoint>();
            asset.transforms = asset.renderer.GetComponentsInChildren<Transform>();
            asset.initialPositions = new Vector3[asset.transforms.Length];
            asset.initialRotations = new Quaternion[asset.transforms.Length];
            for(int i = 0; i < asset.transforms.Length;i++)
            {
                asset.initialPositions[i] = asset.transforms[i].localPosition;
                asset.initialRotations[i] = asset.transforms[i].localRotation;
            }
            asset.connectedAnchors = new Vector3[asset.joints.Length];
            for (int i = 0; i < asset.joints.Length; i++)
            {
                asset.connectedAnchors[i] = asset.joints[i].connectedAnchor;
                asset.joints[i].autoConfigureConnectedAnchor = false;
            }
        }
    }

    public void SetHair(int index)
    {
        if (!IsValidAsset(index, hairAssets)) return;

        foreach (CharacterCustomizationAsset asset in hairAssets)
        {
            asset.renderer.gameObject.SetActive(false);
            for (int i = 0; i < asset.transforms.Length; i++)
            {
                asset.transforms[i].localPosition = asset.initialPositions[i];
                asset.transforms[i].localRotation = asset.initialRotations[i];
            }
        }
        for (int i = 0; i < hairAssets[index].transforms.Length; i++)
        {
            hairAssets[index].transforms[i].localPosition = hairAssets[index].initialPositions[i];
            hairAssets[index].transforms[i].localRotation = hairAssets[index].initialRotations[i];
        }
        for (int i = 0; i < hairAssets[index].joints.Length; i++)
        {
            hairAssets[index].joints[i].connectedAnchor = hairAssets[index].connectedAnchors[i];
        }
        hairAssets[index].renderer.gameObject.SetActive(true);
    }

    [UsedImplicitly]
    public void SetHairById(string id)
    {
        int index = 0;
        foreach (CharacterCustomizationAsset asset in hairAssets)
        {
            if (asset.beamableID == id)
            {
                SetHair(index);
                return;
            }
            index++;
        }
    }

    [UsedImplicitly]
    public void SetHairColorById(string id)
    {
        if (ColorUtility.TryParseHtmlString("#" + id, out Color color))
        {
            foreach (CharacterCustomizationAsset asset in hairAssets)
            {
                asset.renderer.material.SetColor("_BaseColor", color);
                asset.currentColor = color;
            }
        }
    }

    public void SetBodyColorById(string id)
    {
        if (ColorUtility.TryParseHtmlString("#" + id, out Color color))
        {
            foreach (CharacterCustomizationAsset asset in bodyAssets)
            {
                asset.renderer.material.SetColor("_BaseColor", color);
                asset.currentColor = color;
            }
        }
    }

    public bool IsValidAsset(int index, CharacterCustomizationAsset[] assets)
    {
        return (index > -1 && index < assets.Length && assets[index] != null);
    }

    public bool IsValidColor(int index, Color[] colors)
    {
        return (index > -1 && index < colors.Length && colors[index] != null);
    }

}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Beamable;
using Beamable.Api.CloudSaving;
using Beamable.Microservices;
using Beamable.Server.Clients;
using UnityEngine;
using UnityEngine.Events;

public class AchievementCloudSave : MonoBehaviour
{
    private const string FILE_NAME = "achievementsFile.json";
    private const string ACHIEVEMENT_GROUP = "achievement_group.AchievementGroup";
    
    [SerializeField] private AchievementsDictionary achievementsDictionary;

    [SerializeField] private UnityEvent OnUpdateRecieved;
    [SerializeField] private UnityEvent OnErrorRevieved;
    [SerializeField] private UnityEvent<List<AchievementContent>> OnAchievementsLoaded;
    
    private string _filePath = "";
    private IBeamableAPI _beamableAPI;
    private Beamable.Api.CloudSaving.CloudSavingService _cloudSavingService;
    private AchievementsServiceClient _achievementsService;

    private void Awake()
    {
        _achievementsService = new AchievementsServiceClient();
    }

    private void OnEnable()
    {
        SetUpBeamable();
    }

    private async void SetUpBeamable()
    {
        _beamableAPI = await Beamable.API.Instance;
        _cloudSavingService = _beamableAPI.CloudSavingService;
        
        _filePath = $"{_cloudSavingService.LocalCloudDataFullPath}{Path.DirectorySeparatorChar}{FILE_NAME}";
        
        _cloudSavingService.UpdateReceived += 
            CloudSavingService_OnUpdateReceived;
        _cloudSavingService.OnError += CloudSavingService_OnError;

        if(!_cloudSavingService.isInitializing) _cloudSavingService.Init();

        achievementsDictionary = await LoadAchievements();
    }

    private async Task<AchievementsDictionary> LoadAchievements()
    {
        Directory.CreateDirectory(_cloudSavingService.LocalCloudDataFullPath);

        AchievementsDictionary achievementDictionary;

        if (File.Exists(_filePath))
        {
            var achievementJson = File.ReadAllText(_filePath);
            achievementDictionary = JsonUtility.FromJson<AchievementsDictionary>(achievementJson);
        }
        else
        {
            achievementDictionary = new AchievementsDictionary();
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath));
        }
        
        var achievementList = await _achievementsService.CheckAchievements();
        achievementDictionary.achievements.Clear();
        foreach (var achievementKVP in achievementList)
        {
            AchievementsDictionary.Achievement achievement = new AchievementsDictionary.Achievement
            {
                key = achievementKVP.Key,
                value = achievementKVP.Value
            };
            achievementDictionary.achievements.Add(achievement);
        }

        var json = JsonUtility.ToJson(achievementDictionary);
        File.WriteAllText(_filePath, json);
        
        OnAchievementsLoaded?.Invoke(await LoadContentAchievements());
        return achievementDictionary;
    }
    
    private async Task<List<AchievementContent>> LoadContentAchievements()
    {
        var contentService = _beamableAPI.ContentService;
        var rawAchievementGroup = await contentService.GetContent(ACHIEVEMENT_GROUP);
        var achievementGroup = rawAchievementGroup as AchievementGroupContent;
        if (achievementGroup == null) return new List<AchievementContent>();

        var resolvedAchievements = new List<AchievementContent>();
        foreach (var achievement in achievementGroup.achievements)
        {
            resolvedAchievements.Add(await achievement.Resolve());
        }

        return resolvedAchievements;
    }

    public bool CheckIfAchieved(string id)
    {
        var achievements = achievementsDictionary;
        return achievements.achievements.Any(achievement => achievement.key == id && achievement.value);
    }

    private void CloudSavingService_OnUpdateReceived(ManifestResponse manifest)
    {
        Debug.Log($"CloudSavingService_OnUpdateReceived()");
        OnUpdateRecieved?.Invoke();
    }
    
    
    private void CloudSavingService_OnError(CloudSavingError cloudSavingError)
    {
        Debug.Log($"CloudSavingService_OnError() Message = {cloudSavingError.Message}");
        OnErrorRevieved?.Invoke();
    }
    
}

[Serializable]
public class AchievementsDictionary
{
    [Serializable]
    public struct Achievement
    {
        public string key;
        public bool value;
    }

    public List<Achievement> achievements = new List<Achievement>();
}

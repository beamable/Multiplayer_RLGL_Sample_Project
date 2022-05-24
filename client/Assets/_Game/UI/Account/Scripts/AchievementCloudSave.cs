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

    [SerializeField] private UnityEvent OnUpdateReceived;
    [SerializeField] private UnityEvent OnErrorReceived;
    [SerializeField] private UnityEvent<List<AchievementContent>> OnAchievementsLoaded;
    
    private string _filePath = "";
    private BeamContext _context;
    private Beamable.Api.CloudSaving.CloudSavingService _cloudSavingService;

    private void OnEnable()
    {
        SetUpBeamable();
    }

    private async void SetUpBeamable()
    {
        _context = BeamContext.Default;
        await _context.OnReady;
        _cloudSavingService = _context.Api.CloudSavingService;
        
        _filePath = $"{_cloudSavingService.LocalCloudDataFullPath}{Path.DirectorySeparatorChar}{FILE_NAME}";
        
        _cloudSavingService.UpdateReceived += 
            CloudSavingService_OnUpdateReceived;
        _cloudSavingService.OnError += CloudSavingService_OnError;

        if(!_cloudSavingService.isInitializing) await _cloudSavingService.Init();
        _context.Api.NotificationService.Subscribe("achievementEarned", SaveAchievement);

        achievementsDictionary = await LoadAchievements();
        BeamableStatsController.SetCurrentEarnedAchievements(achievementsDictionary.achievements);
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

        var json = JsonUtility.ToJson(achievementDictionary);
        File.WriteAllText(_filePath, json);
        
        OnAchievementsLoaded?.Invoke(await LoadContentAchievements());
        return achievementDictionary;
    }
    
    private async Task<List<AchievementContent>> LoadContentAchievements()
    {
        var contentService = _context.Api.ContentService;
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
        return achievements.achievements.Any(achievement => achievement == id);
    }

    public void SaveAchievement(object achievementId)
    {
        achievementsDictionary.achievements.Add(achievementId.ToString());
        BeamableStatsController.SetCurrentEarnedAchievements(achievementsDictionary.achievements);
        var json = JsonUtility.ToJson(achievementsDictionary);

        if (!Directory.Exists(_filePath))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath));
        }

        File.WriteAllText(_filePath, json);
    }

    private void CloudSavingService_OnUpdateReceived(ManifestResponse manifest)
    {
        Debug.Log($"CloudSavingService_OnUpdateReceived()");
        OnUpdateReceived?.Invoke();
    }
    
    
    private void CloudSavingService_OnError(CloudSavingError cloudSavingError)
    {
        Debug.Log($"CloudSavingService_OnError() Message = {cloudSavingError.Message}");
        OnErrorReceived?.Invoke();
    }
    
}

[Serializable]
public class AchievementsDictionary
{
    public List<string> achievements = new List<string>();
}

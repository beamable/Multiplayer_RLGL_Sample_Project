using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Beamable;
using Beamable.Api.CloudSaving;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class PlayerGraphicsSettings
{
    [Serializable]
    public struct Setting
    {
        public string key;
        public string value;
    }
    public List<Setting> settings = new List<Setting>();
}

[Serializable]
public class PlayerAudioSettings
{
    [Serializable]
    public struct Setting
    {
        public string key;
        public string value;
    }
    public List<Setting> settings = new List<Setting>();
}

[Serializable]
public class PlayerAccessibilitySettings
{
    [Serializable]
    public struct Setting
    {
        public string key;
        public string value;
    }
    public List<Setting> settings = new List<Setting>();
}

public class SettingsCloudSave : MonoBehaviour
{
    [SerializeField] private PlayerGraphicsSettings playerGraphicsSettings = null;
    [SerializeField] private PlayerAudioSettings playerAudioSettings = null;
    [SerializeField] private PlayerAccessibilitySettings playerAccessibilitySettings = null;
 
    [SerializeField] private UnityEvent OnUpdateRecieved;
    [SerializeField] private UnityEvent OnErrorRecieved;
    [SerializeField] private UnityEvent<PlayerGraphicsSettings> OnGraphicsLoaded;
    [SerializeField] private UnityEvent<PlayerAudioSettings> OnAudioLoaded;
    [SerializeField] private UnityEvent<PlayerAccessibilitySettings> OnAccessibilityLoaded;

    private IBeamableAPI _beamableAPI;
    private CloudSavingService _cloudSavingService;
    
    public string GraphicsFileName { get; private set; } = "graphicsFile.json";
    public string AudioFileName { get; private set; } = "audioFile.json";
    public string AccessibilityFileName { get; private set; } = "accessibilityFile.json";

    private void Awake()
    {
        SetUpBeamable();
    }

    private async void SetUpBeamable()
    {
        _beamableAPI = await Beamable.API.Instance;
        _cloudSavingService = _beamableAPI.CloudSavingService;
        
        _cloudSavingService.UpdateReceived += 
            CloudSavingService_OnUpdateReceived;
        _cloudSavingService.OnError += CloudSavingService_OnError;
        
        if(!_cloudSavingService.isInitializing) await _cloudSavingService.Init();
        
        OnGraphicsLoaded?.Invoke(LoadSetting<PlayerGraphicsSettings>(GraphicsFileName));
        OnAudioLoaded?.Invoke(LoadSetting<PlayerAudioSettings>(AudioFileName));
        OnAccessibilityLoaded?.Invoke(LoadSetting<PlayerAccessibilitySettings>(AccessibilityFileName));
    }

    private T LoadSetting<T>(string fileName) where T : new()
    {
        Directory.CreateDirectory(_cloudSavingService.LocalCloudDataFullPath);
        T settings;
        var path = $"{_cloudSavingService.LocalCloudDataFullPath}{Path.DirectorySeparatorChar}{fileName}";
        if (File.Exists(path))
        {
            var json = File.ReadAllText(path);
            settings = JsonUtility.FromJson<T>(json);
        }
        else
        {
            settings = new T();
            
            var json = JsonUtility.ToJson(settings);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
        
            File.WriteAllText(path, json);
        }

        return settings;
    }

    public void SaveSettings<T>(T settings, string fileName)
    {
        var path = $"{_cloudSavingService.LocalCloudDataFullPath}{Path.DirectorySeparatorChar}{fileName}";
        var json = JsonUtility.ToJson(settings);

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
        }

        File.WriteAllText(path, json);
    }

    private void CloudSavingService_OnUpdateReceived(ManifestResponse manifest)
    {
        Debug.Log($"CloudSavingService_OnUpdateReceived()");
        OnUpdateRecieved?.Invoke();
    }
    
    private void CloudSavingService_OnError(CloudSavingError cloudSavingError)
    {
        Debug.Log($"CloudSavingService_OnError() Message = {cloudSavingError.Message}");
        OnErrorRecieved?.Invoke();
    }
}

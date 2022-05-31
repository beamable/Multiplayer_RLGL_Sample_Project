# Cloud Save

### In-Game Usage

Cloud save is used for saving the player's settings on their device and for keeping a record of the player's [Achievements](./Achievements.md). Files for cloud saving the player's settings and achievements are stored in the user's local low data. The settings cloud save updates whenever a setting is changed, and the achievement cloud save updates when the achievements are loaded on the player's profile page.

### Class Breakdown

- **SettingsCloudSave**
    - Has classes for each setting category that contain a list of structs with two string values; one that contains the setting key and the other that contains the setting value.
    - Uses Beamable's Cloud Saving to load and save the player's settings values to the device.
    - Uses Unity events when the settings are loaded to send a signal to update the setting UI.
```csharp
private async void SetUpBeamable()
{
    _context = BeamContext.Default;
    await _context.OnReady;
    _cloudSavingService = _context.Api.CloudSavingService;
    
    _cloudSavingService.UpdateReceived += 
        CloudSavingService_OnUpdateReceived;
    _cloudSavingService.OnError += CloudSavingService_OnError;
    
    if(!_cloudSavingService.isInitializing) await _cloudSavingService.Init();
    
    //Load each setting type and pass them through a UnityEvent for other classes to get their values.
    OnGraphicsLoaded?.Invoke(LoadSetting<PlayerGraphicsSettings>(GraphicsFileName));
    OnAudioLoaded?.Invoke(LoadSetting<PlayerAudioSettings>(AudioFileName));
    OnAccessibilityLoaded?.Invoke(LoadSetting<PlayerAccessibilitySettings>(AccessibilityFileName));
}
```

- **SettingConfig** - Holds the information about each setting. Added as a component to each setting object.
    - Has the setting key that the setting is identified by, the setting's value, the setting's default value, a Unity event for when the value changes, and the type of setting for the kind of value the setting has.
    - The setting types used are slider, toggle, and a list selection; these update the values from the UI.
    - If a value cannot be found for the setting, it will be set to its default value.
```csharp
public void SetSettingValue(float value)
{
    DefaultIfEmpty();
    settingValue = value.ToString();
    OnValueChange?.Invoke(settingKey, settingValue);
}
```

- **SettingsController** - Base class for getting the current settings, default settings, and applying the setting UI values.
    - **AccessibilityCloudSettings** - Gets and applies the settings for the accessibility setting category.
    - **AudioCloudSettings** - Gets and applies the settings for the audio setting category.
    - **GraphicsCloudSettings** - Gets and applies the settings for the graphics setting category.

To avoid the amount of references in the inspector, the SettingConfigsInChildren property gets all of the SettingConfig child objects.

```csharp
protected List<SettingConfig> SettingConfigsInChildren()
{
    var childrenWithConfig = new List<SettingConfig>();
    var children = (from Transform child in transform select child.gameObject).ToList();
    foreach (var child in children)
    {
        var settingConfigs = child.GetComponentsInChildren<SettingConfig>();
        if (settingConfigs == null) continue;
        childrenWithConfig.AddRange(settingConfigs);
    }

    return childrenWithConfig;
}

protected Dictionary<string, string> GetDefaultSettings()
{
    var settingConfigs = SettingConfigsInChildren();

    return settingConfigs.ToDictionary(config => config.settingKey, config => config.defaultValue);
}

protected Dictionary<string, string> GetCurrentSettings()
{
    var settingConfigs = SettingConfigsInChildren();

    return settingConfigs.ToDictionary(config => config.settingKey, config => config.settingValue);
}
```

- **AchievementCloudSave**
    - Uses Beamable's Cloud Saving service to load and save the player's achievement data to the device.
    - Uses Beamable's [Microservice](./Microservices.md) feature to check whether the player has earned any achievements.
    - Uses Beamable's [Content](./Content.md) service to load the achievements contents from a specified achievement_group content.
    - Saves the achievement name and whether or not it has been achieved in a list of a struct that has a string value and a bool value.

    > This is one of the core components to the Achievements feature. This is further documented in the [Achievements documentation](./Achievements.md).

```csharp
private async void SetUpBeamable()
{
    _context = BeamContext.Default;
    await _context.OnReady;
    _cloudSavingService = _context.Api.CloudSavingService;
    
    _filePath = $"{_cloudSavingService.LocalCloudDataFullPath}{Path.DirectorySeparatorChar}{FILE_NAME}";
    
    _cloudSavingService.UpdateReceived += 
        CloudSavingService_OnUpdateReceived;
    _cloudSavingService.OnError += CloudSavingService_OnError;

    if(!_cloudSavingService.isInitializing) _cloudSavingService.Init();
    _context.Api.NotificationService.Subscribe("achievementEarned", SaveAchievement);

    achievementsDictionary = await LoadAchievements();
    BeamableStatsController.SetCurrentEarnedAchievements(achievementsDictionary.achievements);
}
```


> To learn more about the basics of the Cloud Save feature, read more on Beamable's documentation site [here](https://docs.beamable.com/docs/cloud-save-feature-overview).

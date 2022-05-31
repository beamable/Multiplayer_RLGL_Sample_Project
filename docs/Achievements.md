# Achievements

### In-Game Usage

The achievements system uses several of Beamable's features.
Achievements are used to keep track of players' progress toward milestones and any have been reached. Achievements are set by [content](./Content.md) in the Content Manager and saved as [Stats](./Stats.md). A list of achievements are also saved to the device through [Cloud Save](./CloudSave.md) to keep track of them. Achievements can be seen in the player's profile page. 

The player will be notified when they have earned an achievement through the help of Beamable's [Microservices](./Microservices.md) and Notification Service features. When a player earns an achievement, they will see a notification popup appear at the bottom of the screen for a few moments.

## Features Used
### [Content](./Content.md)
Achievements are their own custom content type along with a custom content type of achievement_group, which holds a list of achievements contents.
- **achievement_group** - Holds a list of achievement content objects. This is referenced by the Achievement microservice to identify all of the current "active" achievements.
- **achievements** - Holds the name, description, icon, and requirements for an achievement and whether or not that achievement is secret. Requirements are either a string requirement that can be marked as true or false if met, or a count requirement for a number to be met. These requirements also have a [Stat](./Stats.md) key, indicating which player stat should be tracked to validate that the requirement has been met. Marking an achievement as secret is a way to signal to the UI that the name, description, and icon for the achievement should be masked until the player has earned it.

### [Stats](./Stats.md)
Whether or not an achievement has been obtained is saved as a stat. The stat key corresponds to the one set in the achievements [Content](./Content.md) and the stat value should be a desired value also set by the achievements content. When the stats of the player are updated to fulfill this criteria, the achievement is granted.

### [Cloud Save](./CloudSave.md)
**AchievementCloudSave**
- Uses Beamable's Cloud Save service to load and save the player's achievement data to the device.
- Beamable's [Microservice](./Microservices.md) feature is used to check whether the player has earned any achievements.
- Uses Beamable's [Content](./Content.md) Service to load the achievements contents from a specified achievement_group content.
- Saves the achievement name and whether or not it has been achieved in a list of a struct that has a string value and a bool value.
```csharp
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
```

### [Microservices](./Microservices.md)
The Achievement Service is called to check progress toward achievement requirements, whether achievement conditions have been met, and to save that information into [Cloud Save](./CloudSave.md).

**AchievementsService** - Checks if achievement requirements have been met.
- [Content](./Content.md) service is used to load the content from an achievement group.
```csharp
public async Task<List<AchievementContent>> LoadAchievements()
{
    var rawAchievementGroup = await Services.Content.GetContent(ACHIEVEMENT_GROUP);
    var achievementGroup = rawAchievementGroup as AchievementGroupContent;
    if (achievementGroup == null) return new List<AchievementContent>();

    var resolvedAchievements = new List<AchievementContent>();
    foreach (var achievement in achievementGroup.achievements)
    {
        resolvedAchievements.Add(await achievement.Resolve());
    }

    return resolvedAchievements;
}
```
- [Stat](./Stats.md) service is used to check the content against the corresponding stats and to return a list of achievements and whether or not any achievements have been earned.
```csharp
private async Task<bool> CheckCountAchievement(AchievementContent content)
{
    var stats = await Services.Stats.GetStats(DOMAIN, ACCESS, STAT_TYPE, Context.UserId);
    stats.TryGetValue(content.TotalCountRequirement.Key, out var value);
    if (value == null) return false;

    return !(int.Parse(value) < content.TotalCountRequirement.TotalCount);
}

private async Task<bool> CheckStringAchievement(StringRequirement requirement)
{
    var stats = await Services.Stats.GetStats(DOMAIN, ACCESS, STAT_TYPE, Context.UserId);
    stats.TryGetValue(requirement.Key, out var value);
    return value == bool.TrueString;
}
```

### Notifications
Notifications are used to subscribe to when an achievement has been earned. The `CallbackAction` for when the subscription is assigned to create a popup to appear with the achievement's information on it. The achievement notification system publishes through a [Microservice](./Microservices.md), so the callback happens when it is called through the microservice.

**AchievementsService** - Sends the `NotifyPlayer` call after stats have been checked against corresponding achievements to see if a new achievement has been earned.
```csharp
public async Task AchievementEarnedNotification(long dbid, object achievementId)
{
	await Services.Notifications.NotifyPlayer(dbid, "achievementEarned", achievementId);
}
```

**AchievementCloudSave** - Subscribes to the `AchievementEarned` notification. When a callback for it happens, the [Cloud Save](./CloudSave.md) updates Achievements to mark the new achievement as having been earned.
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

**AchievementEarnedSpawner** - Subscribes to the `AchievementEarned` notification. When a callback for it happens, the spawner instantiates a popup with the information of the newly earned achievement displayed.
```csharp
private async void SpawnAchievementPopup(string achievementId)
{
    var content = await LoadAchievementFromId(achievementId);
    var popup = Instantiate(popupPrefab, transform);
    popup.GetComponent<AchievementEarnedPopupUI>().SetUp(content);
    Destroy(popup, popupTime);
}
```


> To learn more about basic usage of Beamable's features, it can be found on Beamable's docs:
> - [Content](https://docs.beamable.com/docs/content-feature-overview)
> - [Stats](https://docs.beamable.com/docs/stats-feature-overview)
> - [Cloud Save](https://docs.beamable.com/docs/cloud-save-feature-overview)
> - [Microservices](https://docs.beamable.com/docs/microservices-feature-overview)
> - [Notifcations](https://docs.beamable.com/docs/notifications-feature-overview)

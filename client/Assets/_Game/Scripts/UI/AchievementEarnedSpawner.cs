using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beamable;
using Beamable.Microservices;
using UnityEngine;

public class AchievementEarnedSpawner : MonoBehaviour
{
    [SerializeField] private GameObject popupPrefab;
    [SerializeField] private float popupTime = 4f;

    private IBeamableAPI _beamableAPI;

    private const string ACHIEVEMENT_GROUP = "achievement_group.AchievementGroup";

    private void Awake()
    {
        SetUpBeamable();
    }

    private async void SetUpBeamable()
    {
        _beamableAPI = await API.Instance;
        _beamableAPI.NotificationService.Subscribe("achievementEarned", AchievementEarnedCallback);
    }

    private void AchievementEarnedCallback(object achievementId)
    {
        SpawnAchievementPopup(achievementId.ToString());
    }

    private async void SpawnAchievementPopup(string achievementId)
    {
        var content = await LoadAchievementFromId(achievementId);
        var popup = Instantiate(popupPrefab, transform);
        popup.GetComponent<AchievementEarnedPopupUI>().SetUp(content);
        Destroy(popup, popupTime);
    }

    private async Task<AchievementContent> LoadAchievementFromId(string achievementId)
    {
        var contentService = _beamableAPI.ContentService;
        var rawAchievementGroup = await contentService.GetContent(ACHIEVEMENT_GROUP);
        var achievementGroup = rawAchievementGroup as AchievementGroupContent;
        if (achievementGroup == null) return ScriptableObject.CreateInstance<AchievementContent>();
        
        foreach (var achievement in achievementGroup.achievements.Where(achievement => achievement.Id == achievementId))
        {
            return await achievement.Resolve();
        }

        return ScriptableObject.CreateInstance<AchievementContent>();
    }
}

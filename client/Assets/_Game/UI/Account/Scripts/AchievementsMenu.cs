using System.Collections.Generic;
using Beamable.Microservices;
using JetBrains.Annotations;
using ListView;
using TMPro;
using UnityEngine;

namespace _Game.UI.Account.Scripts
{
    public class AchievementsMenu : MonoBehaviour
    {
        [SerializeField]
        private ListViewComponent achievementsListView;
        [SerializeField]
        private TextMeshProUGUI completedCount;
        [SerializeField]
        private AchievementCloudSave achievementCloudSave;
        
        [UsedImplicitly]
        public void UpdateAchievementsList(List<AchievementContent> achievements)
        {
            var cardData = new ListViewData();
            foreach (var achievement in achievements)
            {
                cardData.Add(new ListItem
                {
                    Title = achievement.Name,
                    PropertyBag = new Dictionary<string, object>()
                    {
                        {"description", achievement.Description},
                        {"icon", achievement.Icon},
                        {"total", achievement.TotalCountRequirement},
                        {"complete", achievementCloudSave.CheckIfAchieved(achievement.Id)},
                        {"secret", achievement.IsSecret}
                    }
                });
            }
            achievementsListView.Build(cardData);
            UpdateCompletedCount(achievements);
        }

        private void UpdateCompletedCount(List<AchievementContent> achievements)
        {
            var amountCompleted = 0;
            foreach (var achievement in achievements)
            {
                if (achievementCloudSave.CheckIfAchieved(achievement.Id))
                {
                    amountCompleted++;
                }
            }
            var total = achievements.Count;
            completedCount.text = $"{amountCompleted}/{total}";
        }
    }
}

using System.Collections.Generic;
using Beamable.Microservices;
using UnityEngine;
using UnityEngine.Events;

namespace _Game.UI.Account.Scripts
{
    public class AchievementsController : MonoBehaviour
    {
        [SerializeField]
        private AchievementGroupRef achievementGroupRef;
        [SerializeField]
        private UnityEvent<List<AchievementContent>> OnAchievementsUpdated;
        
        private async void Awake()
        {
            if (achievementGroupRef == null) return;
            var group = await achievementGroupRef.Resolve();
            var achievements = new List<AchievementContent>();
            foreach (var achievement in group.achievements)
            {
                achievements.Add(await achievement.Resolve());
            }
            OnAchievementsUpdated?.Invoke(achievements);
        }
    }
}
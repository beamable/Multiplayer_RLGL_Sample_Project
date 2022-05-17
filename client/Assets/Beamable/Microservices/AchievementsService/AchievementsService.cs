using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beamable.Server;

namespace Beamable.Microservices
{
	[Microservice("AchievementsService")]
	public class AchievementsService : Microservice
	{
		private const string ACCESS = "public";
		private const string DOMAIN = "client";
		private const string STAT_TYPE = "player";
		private const string ACHIEVEMENT_GROUP = "achievement_group.AchievementGroup";

		private async Task<bool> CheckCountAchievement(int countRequirement, string key)
		{
			var stats = await Services.Stats.GetStats(DOMAIN, ACCESS, STAT_TYPE, Context.UserId);
			stats.TryGetValue(key, out var value);
			if (value == null) return false;

			return !(int.Parse(value) < countRequirement);
		}

		private async Task<bool> CheckStringAchievement(string key)
		{
			var stats = await Services.Stats.GetStats(DOMAIN, ACCESS, STAT_TYPE, Context.UserId);
			stats.TryGetValue(key, out var value);
			return value == bool.TrueString;
		}

		[ClientCallable]
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
		
		[ClientCallable]
		public async Task<Dictionary<string, bool>> CheckAchievements()
		{
			Dictionary<string, bool> achievementDictionary = new Dictionary<string, bool>();
			var achievements = await LoadAchievements();
			foreach (var achievement in achievements)
			{
				if (achievement.TotalCountRequirement.TotalCount != 0)
				{
					achievementDictionary.Add(achievement.Id, await CheckCountAchievement(achievement.TotalCountRequirement.TotalCount,
						achievement.TotalCountRequirement.Key));
				}
				else if (achievement.StringRequirements.Count > 0)
				{
					achievementDictionary.Add(achievement.Id, await CheckStringAchievement(achievement.StringRequirements.First().Key));
				}
			}

			return achievementDictionary;
		}
	}
}

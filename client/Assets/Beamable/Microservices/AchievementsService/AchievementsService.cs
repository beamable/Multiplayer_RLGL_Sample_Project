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

		private List<string> _achievedAchievements = new List<string>();

		[ClientCallable]
		public async Task<bool> CheckAchievement(AchievementContent achievement, string key)
		{
			if (achievement.TotalCountRequirement.TotalCount != 0 && achievement.StringRequirements.Count > 0)
			{
				var hasStringKey = achievement.StringRequirements.Any(stringRequirement => stringRequirement.Key == key);
				if (achievement.TotalCountRequirement.Key == key || hasStringKey)
				{
					return await CheckCountAchievement(achievement) && await CheckStringAchievements(achievement);
				}
			}
			if (achievement.TotalCountRequirement.TotalCount != 0)
			{
				if (achievement.TotalCountRequirement.Key == key)
				{
					return await CheckCountAchievement(achievement);
				}
			}

			if (achievement.StringRequirements.Count <= 0) return false;
			{
				if (achievement.StringRequirements.Any(stringRequirement => stringRequirement.Key == key))
				{
					return await CheckStringAchievements(achievement);
				}
			}

			return false;
		}

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

		private async Task<bool> CheckStringAchievements(AchievementContent content)
		{
			foreach (var stringRequirement in content.StringRequirements)
			{
				if (!await CheckStringAchievement(stringRequirement))
				{
					return false;
				}
			}

			return true;
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
		public async Task<List<string>> CheckAchievements(string key)
		{
			List<string> achievementsEarned = new List<string>();
			var achievements = await LoadAchievements();
			foreach (var achievement in achievements)
			{
				if (!await CheckAchievement(achievement, key)) continue;
				if (!achievementsEarned.Contains(achievement.Id))
				{
					achievementsEarned.Add(achievement.Id);
				}
			}

			return achievementsEarned;
		}

		[ClientCallable]
		public async Task AchievementEarnedNotification(long dbid, object achievementId)
		{
			await Services.Notifications.NotifyPlayer(dbid, "achievementEarned", achievementId);
		}
	}
}

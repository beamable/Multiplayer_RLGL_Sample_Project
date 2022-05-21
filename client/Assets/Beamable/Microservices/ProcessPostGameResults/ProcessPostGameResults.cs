using System.Linq;
using Beamable.Common;
using Beamable.Common.Inventory;
using UnityEngine;

namespace Beamable.Microservices
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using Beamable.Common.Shop;
	using Beamable.Server;
	
	/// <summary>
	/// Class to process post game results
	/// </summary>
	[Microservice("ProcessPostGameResults")]
	public class ProcessPostGameResults : Microservice
	{
		private const int MaxWinners = 7;
		private const string Score = "score";
		private const string Rank = "rank";
		private const string GamesPlayed = "games_played";
		private const string Store = "stores.HeadStore";
		private const string LeaderboardName = "leaderboards.ranked";
		private const string Kills = "kills";
		private const string Access = "public";
		private const string Domain = "client";
		private const string StatType = "player";
		private const string Alias = "alias";

		private int _dropRate = 90;
		private string _coins = "currency.photons";
		private PostGameResult _gameResult = new PostGameResult();

		private async Task GetSettingsFromRemoteConfig()
		{
			var configSettings = await this.Services.RealmConfig.GetRealmConfigSettings();
			var config = configSettings["gameSettings"];
			_dropRate = int.Parse(config.GetSetting("dropRate"));
			_coins = config.GetSetting("coinType");
		}
		
		/// <summary>
		/// Get a legend of values as rank points for each placement from Realm Config
		/// </summary>
		/// <returns>List of (int) rank points per placement</returns>
		private async Task<List<int>> GetLegend()
		{
			var configSettings = await this.Services.RealmConfig.GetRealmConfigSettings();
			var config = configSettings["gameSettings"];
			var rankLegend = config.GetSetting("rankLegend");
			BeamableLogger.Log($"Rank Legend: {rankLegend}");
			var legend = JsonUtility.FromJson<List<int>>(rankLegend);
			return legend.Count == 0 ? new List<int>() {85, 70, 55, 40, 25, 10, -5, -15} : legend;
		}

		/// <summary>
		/// Calculate new rank from old rank and add it to game result
		/// </summary>
		/// <param name="stats"></param>
		/// <param name="positionScore"></param>
		/// <returns></returns>
		private void SetRanks(Dictionary<string,string> stats, int positionScore, int score)
		{
			var totalKills = stats.ContainsKey(Kills) ? int.Parse(stats[Kills]) : 0;
			var gamesPlayed = stats.ContainsKey(GamesPlayed) ? int.Parse(stats[GamesPlayed]) : 1;
			BeamableLogger.Log($"Games Played: {gamesPlayed}");
			var previousRank = stats.ContainsKey(Rank) ? int.Parse(stats[Rank]) : 1000;
			BeamableLogger.Log($"Previous Rank: {previousRank}");
			var newRank = positionScore + previousRank;
			BeamableLogger.Log($"New Rank: {newRank}");
			_gameResult.OldRank = previousRank;
			_gameResult.NewRank = newRank;
			_gameResult.GamesPlayed = gamesPlayed + 1;

			if (_gameResult.NewRank < 200)
			{
				_gameResult.NewRank = 200;
			}

			//Grant 2 points for every kill.
			if (totalKills > 0)
			{
				for (var i = 0; i < totalKills; i++)
				{
					_gameResult.NewRank += 2; 
				}
			}
			
			if (score == 0) return;
			
			//Grant 1 extra rank point per 100 score.
			for (var i = 0; i < score; i += 100)
			{
				_gameResult.NewRank++;
			}
		}

		/// <summary>
		/// Grant currency rewards
		/// </summary>
		/// <param name="finishPosition"></param>
		/// <returns></returns>
		private async Task GrantCurrencyRewards(int finishPosition)
		{
			await this.Services.Inventory.AddCurrency(_coins, 100);
			_gameResult.Rewards.Add(new Reward()
			{
				IsItem = false,
				Amount = finishPosition < 8 ? 100 : 200,
				CurrencyType = _coins,
				ItemContentId = string.Empty
			});
		}

		/// <summary>
		/// Grant Items if needed
		/// </summary>
		/// <returns></returns>
		private async Task GrantItemRewards()
		{
			var inventory = await this.Services.Inventory.GetItems<ItemContent>();
			
			var headStore = await this.Services.Content.GetContent(Store, typeof(StoreContent));
			var headStoreRef = headStore as StoreContent;
			if (headStoreRef == null) return;
			
			var listings = headStoreRef.listings;
			var rnd = new Random();
			var val = rnd.Next(0, 100);
			if (val >= _dropRate) return;
			
			//Dropped.
			var itemRnd = new Random();
			var itemIndex = itemRnd.Next(0, listings.Count);
			var item = await listings[itemIndex].Resolve();
			var itemContentId = item.offer.obtainItems[0].contentId;

			if (inventory.FindIndex(i => i.ItemContent.Id == itemContentId.Id) > -1)
			{
				//Don't grant because the player already owns this.
				return;
			}
			
			var properties = item.offer.obtainItems[0].properties.ToDictionary(property => 
				property.name, property => property.value);
			await this.Services.Inventory.AddItem(itemContentId, properties);
			
			_gameResult.Rewards.Add(new Reward()
			{
				IsItem = true,
				ItemContentId = itemContentId
			});
		}

		/// <summary>
		/// Post Rank to leaderboard
		/// </summary>
		/// <param name="newStats"></param>
		/// <returns></returns>
		private async Task PostLeaderboardScore(Dictionary<string,object> newStats)
		{
			var currentLb = await this.Services.Leaderboards.GetBoard(LeaderboardName,1,100);
			if (currentLb != null)
			{
				await this.Services.Leaderboards.SetScore(LeaderboardName, (double) _gameResult.NewRank, newStats);
			}
		}

		/// <summary>
		/// Post Stats to leaderboard
		/// </summary>
		/// <param name="currentStats"></param>
		/// <param name="newStats"></param>
		/// <returns></returns>
		private async Task PostStats(Dictionary<string,string> currentStats, Dictionary<string, object> newStats)
		{
			var stats = new Dictionary<string, string>()
			{
				{Score, newStats[Score].ToString() },
				{Rank,newStats[Rank].ToString()},
				{GamesPlayed,newStats[GamesPlayed].ToString()}
			};
			if (!currentStats.ContainsKey(Alias))
			{
				stats.Add(Alias,newStats[Alias].ToString());
			}
			await this.Services.Stats.SetStats(Access, stats);
		}

		/// <summary>
		/// Process Game results
		/// </summary>
		/// <param name="score"></param>
		/// <param name="finishPosition"></param>
		/// <param name="totalKills"></param>
		/// <returns></returns>
		[ClientCallable]
		public async Task<PostGameResult> ProcessResults(int score, int finishPosition, int totalKills)
		{
			_gameResult = new PostGameResult {Rewards = new List<Reward>()};
			
			if (this.Context == null)
			{
				_gameResult.errorMessage = "this.Context is null";
				return _gameResult;
			}

			await GetSettingsFromRemoteConfig();
			
			_gameResult.PlayerId = this.Context.UserId;
			BeamableLogger.Log("Calling Stats");
			var stats = await this.Services.Stats.GetStats(Domain, Access, StatType, this.Context.UserId).Error((error) =>
			{
				_gameResult.errorMessage = error.Message;
				_gameResult.errorStack = error.StackTrace;
			});
			BeamableLogger.Log("Stats Call Completed");
			BeamableLogger.Log(JsonUtility.ToJson(stats));
			
			var positionRankEloLegend = await GetLegend();
			BeamableLogger.Log(JsonUtility.ToJson(positionRankEloLegend));
			BeamableLogger.Log($"FinishPosition:{finishPosition}");
			var index = Math.Min(finishPosition, MaxWinners) >= 0 ? Math.Min(finishPosition, MaxWinners) : 0;
			var positionScore = positionRankEloLegend[index];
			BeamableLogger.Log($"Position Score: {positionScore}");
			
			BeamableLogger.Log("Setting Ranks"); 
			SetRanks(stats, positionScore, score);
			
			BeamableLogger.Log("Granting Stuff");
			await GrantCurrencyRewards(finishPosition);
			await GrantItemRewards();
			_gameResult.Score = score;
			
			BeamableLogger.Log("Posting Leaderboards & Stats");
			var statsToPost = new Dictionary<string, object>()
			{
				{GamesPlayed, _gameResult.GamesPlayed},
				{Rank, _gameResult.NewRank},
				{Score, score},
				{Kills, totalKills},
				{Alias, stats.ContainsKey(Alias) ? stats[Alias] : "Anonymous"}
			};
			
			BeamableLogger.Log(JsonUtility.ToJson(statsToPost));
			
			await PostLeaderboardScore(statsToPost);
			await PostStats(stats, statsToPost);

			BeamableLogger.Log(JsonUtility.ToJson(_gameResult));
			return _gameResult;
		}
	}
}

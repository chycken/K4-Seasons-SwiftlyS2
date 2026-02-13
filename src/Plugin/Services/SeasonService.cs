using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace K4Seasons;

public sealed partial class Plugin
{
	public sealed class SeasonService(DatabaseService database)
	{
		private readonly DatabaseService _database = database;

		public int CurrentSeasonId { get; private set; }
		public DateTime SeasonStart { get; private set; }
		public DateTime SeasonEnd { get; private set; }
		public SeasonConfig? CurrentConfig { get; private set; }

		public async Task InitializeAsync()
		{
			var activeSeason = await _database.GetActiveSeasonAsync();

			if (activeSeason != null)
			{
				CurrentSeasonId = activeSeason.Id;
				SeasonStart = activeSeason.StartDate;
				SeasonEnd = activeSeason.EndDate;
				LoadSeasonConfig(activeSeason.Id);

				if (DateTime.UtcNow > SeasonEnd)
					await TransitionSeasonAsync();
			}
			else
			{
				await CreateFirstSeasonAsync();
			}
		}

		public void LoadSeasonConfig(int seasonId)
		{
			var filePath = Path.Combine(Core.PluginPath, $"season_{seasonId}.json");

			if (!File.Exists(filePath))
			{
				Core.Logger.LogError("Season config file not found: {Path}. Please create it based on the shipped season_1.json template.", filePath);
				CurrentConfig = new SeasonConfig();
				return;
			}

			try
			{
				var json = File.ReadAllText(filePath);
				CurrentConfig = JsonSerializer.Deserialize<SeasonConfig>(json, new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true,
					ReadCommentHandling = JsonCommentHandling.Skip
				});
				Core.Logger.LogInformation("Loaded season config for season {Id}.", seasonId);
			}
			catch (Exception ex)
			{
				Core.Logger.LogError(ex, "Failed to load season config for season {Id}.", seasonId);
				CurrentConfig = new SeasonConfig();
			}
		}

		private async Task CreateFirstSeasonAsync()
		{
			LoadSeasonConfig(1);
			var durationDays = CurrentConfig?.DurationDays > 0 ? CurrentConfig.DurationDays : 90;

			var season = new DbSeason
			{
				SeasonName = CurrentConfig?.SeasonName ?? "Season 1",
				StartDate = DateTime.UtcNow,
				EndDate = DateTime.UtcNow.AddDays(durationDays),
				IsActive = true
			};

			var id = await _database.CreateSeasonAsync(season);
			CurrentSeasonId = id > 0 ? id : 1;
			SeasonStart = season.StartDate;
			SeasonEnd = season.EndDate;
		}

		public async Task TransitionSeasonAsync()
		{
			Core.Logger.LogInformation("Season {Id} has ended. Transitioning...", CurrentSeasonId);

			var oldSeason = await _database.GetActiveSeasonAsync();

			if (oldSeason != null)
			{
				oldSeason.IsActive = false;
				await _database.UpdateSeasonAsync(oldSeason);
			}

			await _database.ResetSeasonDataAsync();

			var newSeasonId = CurrentSeasonId + 1;
			LoadSeasonConfig(newSeasonId);
			var durationDays = CurrentConfig?.DurationDays > 0 ? CurrentConfig.DurationDays : 90;

			var newSeason = new DbSeason
			{
				SeasonName = CurrentConfig?.SeasonName ?? $"Season {newSeasonId}",
				StartDate = DateTime.UtcNow,
				EndDate = DateTime.UtcNow.AddDays(durationDays),
				IsActive = true
			};

			var id = await _database.CreateSeasonAsync(newSeason);
			CurrentSeasonId = id > 0 ? id : newSeasonId;
			SeasonStart = newSeason.StartDate;
			SeasonEnd = newSeason.EndDate;
		}

		public void ExecuteLevelRewards(SeasonPlayer player, int newLevel)
		{
			if (CurrentConfig?.Rewards == null)
				return;

			for (var level = 1; level <= newLevel; level++)
			{
				if (player.ClaimedBattlePassLevels.Contains(level))
					continue;

				if (!CurrentConfig.Rewards.TryGetValue(level, out var reward))
					continue;

				if (reward.BattlePassOnly && !player.HasBattlePass)
					continue;

				player.ClaimedBattlePassLevels.Add(level);
				Task.Run(async () => await _database.InsertClaimedLevelAsync(player.SteamId, level));

				var localizer = Core.Translation.GetPlayerLocalizer(player.Player);
				player.Player.SendChat($"{localizer["k4.general.prefix"]} {localizer["k4.chat.reward_claimed", level, reward.Name]}");

				foreach (var command in reward.Commands)
				{
					var replaced = ReplacePlaceholders(player, command);
					Core.Engine.ExecuteCommand(replaced);
				}

				foreach (var permission in reward.Permissions)
				{
					Core.Permission.AddPermission(player.SteamId, permission);
				}
			}
		}

		public void GiveRetrospectiveRewards(SeasonPlayer player)
		{
			var currentLevel = player.GetLevel(Config.CurrentValue);
			ExecuteLevelRewards(player, currentLevel);
		}

		private static string ReplacePlaceholders(SeasonPlayer player, string command) =>
			command
				.Replace("{name}", player.UserName)
				.Replace("{steamid64}", player.SteamId.ToString())
				.Replace("{steamid}", player.SteamId.ToString())
				.Replace("{slot}", player.Player.Slot.ToString())
				.Replace("{userid}", player.Player.PlayerID.ToString())
				.Replace("u0022", "\"");
	}
}

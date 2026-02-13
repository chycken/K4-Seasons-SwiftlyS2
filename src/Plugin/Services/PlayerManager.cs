using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using SwiftlyS2.Shared.Players;

namespace K4Seasons;

public sealed partial class Plugin
{
	public sealed class PlayerManager(DatabaseService database, MissionService missionService, SeasonService seasonService, ToplistService toplistService)
	{
		private readonly ConcurrentDictionary<ulong, SeasonPlayer> _players = new();
		private readonly DatabaseService _database = database;
		private readonly SeasonService _seasonService = seasonService;
		private readonly ToplistService _toplistService = toplistService;

		public MissionService MissionService { get; } = missionService;

		public int ActivePlayerCount => _players.Values.Count(p => p.IsValid && p.Player.Controller?.Team > Team.Spectator);

		public IEnumerable<SeasonPlayer> AllPlayers => _players.Values;

		public SeasonPlayer GetOrCreatePlayer(IPlayer player)
		{
			return _players.GetOrAdd(player.SteamID, _ =>
			{
				var sp = new SeasonPlayer
				{
					SteamId = player.SteamID,
					Player = player,
					IsVip = CheckVipStatus(player.SteamID)
				};

				Task.Run(async () => await LoadPlayerAsync(sp));
				return sp;
			});
		}

		public SeasonPlayer? GetPlayer(IPlayer player) =>
			_players.TryGetValue(player.SteamID, out var sp) ? sp : null;

		public SeasonPlayer? GetPlayer(ulong steamId) =>
			_players.TryGetValue(steamId, out var sp) ? sp : null;

		public void RemovePlayer(ulong steamId)
		{
			if (_players.TryRemove(steamId, out var player) && player.IsLoaded)
				Task.Run(async () => await SavePlayerAsync(player));
		}

		private async Task LoadPlayerAsync(SeasonPlayer player)
		{
			try
			{
				var dbPlayer = await _database.GetPlayerAsync(player.SteamId);

				if (dbPlayer != null)
				{
					player.Experience = dbPlayer.Experience;
					player.BattlePassPurchased = dbPlayer.BattlePassPurchased;
					player.Streak = dbPlayer.Streak;
					player.Prestige = dbPlayer.Prestige;
					player.Rerolls = dbPlayer.Rerolls;
					player.RerollResetDate = dbPlayer.RerollResetDate;
					player.ActiveTime = dbPlayer.ActiveTime;
					player.ClaimedBattlePassLevels = await _database.GetClaimedLevelsAsync(player.SteamId);
				}
				else
				{
					await _database.UpsertPlayerAsync(player);
				}

				var missions = await _database.GetPlayerMissionsAsync(player.SteamId);
				player.PersonalMissions.AddRange(missions);

				if (Config.CurrentValue.CatchUp.Enabled && player.HasBattlePass)
					player.CatchupTargetXP = await _database.GetAverageBattlePassXPAsync();

				player.IsLoaded = true;

				Core.Scheduler.NextWorldUpdate(() =>
				{
					if (!player.IsValid)
						return;

					Task.Run(async () =>
					{
						var assigned = await MissionService.AssignDailyMissionsAsync(player);

						if (assigned > 0)
						{
							Core.Scheduler.NextWorldUpdate(() =>
							{
								if (!player.IsValid)
									return;

								var loc = Core.Translation.GetPlayerLocalizer(player.Player);
								player.Player.SendChat($"{loc["k4.general.prefix"]} {loc["k4.chat.missions_assigned", assigned]}");
							});
						}
					});

					NotifyNewPlayer(player);
					_seasonService.GiveRetrospectiveRewards(player);
				});
			}
			catch (Exception ex)
			{
				Core.Logger.LogError(ex, "Failed to load player {SteamId}", player.SteamId);
				player.IsLoaded = true;
			}
		}

		public void AddExperience(SeasonPlayer player, long baseAmount)
		{
			var config = Config.CurrentValue;
			var multiplier = player.GetCurrentMultiplier(config, _toplistService.Toplist);
			var amount = (long)(baseAmount * multiplier);

			var oldLevel = player.GetLevel(config);
			player.Experience += amount;
			var newLevel = player.GetLevel(config);

			var maxLevel = player.MaxLevel(config);

			if (newLevel > maxLevel)
			{
				player.Experience = SeasonPlayer.GetRequiredExperience(config, maxLevel);
				newLevel = maxLevel;
			}

			if (newLevel > oldLevel)
			{
				Core.Scheduler.NextWorldUpdate(() =>
				{
					if (!player.IsValid)
						return;

					_seasonService.ExecuteLevelRewards(player, newLevel);

					var localizer = Core.Translation.GetPlayerLocalizer(player.Player);
					player.Player.SendChat($"{localizer["k4.general.prefix"]} {localizer["k4.chat.level_up", newLevel]}");

					if (config.Prestige.Enabled && newLevel >= maxLevel)
					{
						if (player.Prestige < config.Prestige.LevelCap)
							player.Player.SendChat($"{localizer["k4.general.prefix"]} {localizer["k4.chat.prestige_available"]}");
					}
				});
			}
		}

		public void ActivateBattlePass(SeasonPlayer player)
		{
			player.BattlePassPurchased = DateTime.UtcNow;

			Task.Run(async () =>
			{
				if (Config.CurrentValue.CatchUp.Enabled)
					player.CatchupTargetXP = await _database.GetAverageBattlePassXPAsync();
			});

			_seasonService.GiveRetrospectiveRewards(player);

			Core.Scheduler.NextWorldUpdate(() =>
			{
				if (!player.IsValid)
					return;

				Task.Run(async () =>
				{
					var assigned = await MissionService.AssignDailyMissionsAsync(player);

					if (assigned > 0)
					{
						Core.Scheduler.NextWorldUpdate(() =>
						{
							if (!player.IsValid)
								return;

							var loc = Core.Translation.GetPlayerLocalizer(player.Player);
							player.Player.SendChat($"{loc["k4.general.prefix"]} {loc["k4.chat.missions_assigned", assigned]}");
						});
					}
				});
			});
		}

		public void Prestige(SeasonPlayer player)
		{
			var config = Config.CurrentValue;

			if (!config.Prestige.Enabled)
				return;

			if (player.Prestige >= config.Prestige.LevelCap)
				return;

			var maxLevel = player.MaxLevel(config);
			var currentLevel = player.GetLevel(config);

			if (currentLevel < maxLevel)
				return;

			player.Prestige++;
			player.Experience = 0;
			player.ClaimedBattlePassLevels.Clear();
			Task.Run(async () => await _database.DeleteClaimedLevelsAsync(player.SteamId));
		}

		public async Task SavePlayerAsync(SeasonPlayer player)
		{
			await _database.UpsertPlayerAsync(player);

			foreach (var m in player.PersonalMissions.Where(m => m.Id > 0))
				await _database.UpdateMissionAsync(m);
		}

		public async Task SaveAllPlayersAsync()
		{
			foreach (var player in _players.Values.Where(p => p.IsLoaded))
				await SavePlayerAsync(player);

			await MissionService.SaveAllMissionsAsync(_players.Values);
		}

		public void ProcessActiveTime()
		{
			foreach (var player in _players.Values.Where(p => p.IsValid && p.IsLoaded))
				player.ActiveTime++;
		}

		public async Task CheckBattlePassUpdatesAsync()
		{
			// Collect all players without battle pass
			var playersWithoutBP = _players.Values
				.Where(p => p.IsValid && p.IsLoaded && !p.HasBattlePass)
				.ToDictionary(p => p.SteamId);

			if (playersWithoutBP.Count == 0)
				return;

			// Single optimized query for all players
			var battlePassData = await _database.GetBattlePassDataAsync(playersWithoutBP.Keys);

			foreach (var (steamId, purchaseDate) in battlePassData)
			{
				if (!playersWithoutBP.TryGetValue(steamId, out var player))
					continue;

				player.BattlePassPurchased = purchaseDate;

				Core.Scheduler.NextWorldUpdate(() =>
				{
					if (!player.IsValid)
						return;

					Task.Run(async () =>
					{
						// Give retrospective rewards and assign extra missions
						_seasonService.GiveRetrospectiveRewards(player);

						if (Config.CurrentValue.CatchUp.Enabled)
							player.CatchupTargetXP = await _database.GetAverageBattlePassXPAsync();

						var assigned = await MissionService.AssignDailyMissionsAsync(player);

						Core.Scheduler.NextWorldUpdate(() =>
						{
							if (!player.IsValid)
								return;

							var localizer = Core.Translation.GetPlayerLocalizer(player.Player);
							player.Player.SendChat($"{localizer["k4.general.prefix"]} {localizer["k4.chat.bp_activated"]}");

							if (assigned > 0)
								player.Player.SendChat($"{localizer["k4.general.prefix"]} {localizer["k4.chat.missions_assigned", assigned]}");
						});
					});
				});
			}
		}

		public void Clear() => _players.Clear();

		private static bool CheckVipStatus(ulong steamId)
		{
			var flags = Config.CurrentValue.Vip.Flags;

			if (flags.Count == 0)
				return false;

			return flags.Any(f => Core.Permission.PlayerHasPermission(steamId, f));
		}

		private static void NotifyNewPlayer(SeasonPlayer player)
		{
			if (!player.IsValid)
				return;

			var localizer = Core.Translation.GetPlayerLocalizer(player.Player);
			var command = Config.CurrentValue.Commands.Season.Command;
			player.Player.SendChat($"{localizer["k4.general.prefix"]} {localizer["k4.chat.welcome", command]}");
		}
	}
}

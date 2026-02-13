using SwiftlyS2.Core.Menus.OptionsBase;
using SwiftlyS2.Shared.Menus;
using SwiftlyS2.Shared.Translation;

namespace K4Seasons;

public sealed partial class Plugin
{
	private void ShowMainMenu(SeasonPlayer player)
	{
		var localizer = Core.Translation.GetPlayerLocalizer(player.Player);
		var config = Config.CurrentValue;

		var builder = Core.MenusAPI.CreateBuilder()
			.Design.SetMenuTitle(localizer["k4.menu.title"])
			.Design.SetMenuTitleVisible(true)
			.Design.SetMenuFooterVisible(true)
			.Design.SetGlobalScrollStyle(MenuOptionScrollStyle.LinearScroll);

		var infoBtn = new ButtonMenuOption($"<font color='#4A90D9'>{localizer["k4.menu.info"]}</font>");
		infoBtn.Click += (_, _) =>
		{
			ShowSeasonInfo(player, localizer);
			return ValueTask.CompletedTask;
		};
		builder.AddOption(infoBtn);

		var profileBtn = new ButtonMenuOption($"<font color='#4CAF50'>{localizer["k4.menu.profile"]}</font>");
		profileBtn.Click += (_, _) =>
		{
			ShowProfile(player, localizer);
			return ValueTask.CompletedTask;
		};
		builder.AddOption(profileBtn);

		var missionsBtn = new ButtonMenuOption($"<font color='#FF9800'>{localizer["k4.menu.missions"]}</font>");
		missionsBtn.Click += (_, _) =>
		{
			ShowMissionsMenu(player, localizer);
			return ValueTask.CompletedTask;
		};
		builder.AddOption(missionsBtn);

		var bpBtn = new ButtonMenuOption($"<font color='#E91E63'>{localizer["k4.menu.battlepass"]}</font>");
		bpBtn.Click += (_, _) =>
		{
			ShowBattlePassInfo(player, localizer);
			return ValueTask.CompletedTask;
		};
		builder.AddOption(bpBtn);

		var levelsBtn = new ButtonMenuOption($"<font color='#9C27B0'>{localizer["k4.menu.levels"]}</font>");
		levelsBtn.Click += (_, _) =>
		{
			ShowLevelList(player, localizer);
			return ValueTask.CompletedTask;
		};
		builder.AddOption(levelsBtn);

		var toplistBtn = new ButtonMenuOption($"<font color='#FFC107'>{localizer["k4.menu.toplist"]}</font>");
		toplistBtn.Click += (_, _) =>
		{
			ShowToplist(player, localizer);
			return ValueTask.CompletedTask;
		};
		builder.AddOption(toplistBtn);

		var mainMenu = builder.Build();
		Core.MenusAPI.OpenMenuForPlayer(player.Player, mainMenu);
	}

	private void ShowSeasonInfo(SeasonPlayer player, ILocalizer localizer)
	{
		Core.Scheduler.NextWorldUpdate(() =>
		{
			if (!player.IsValid)
				return;

			var config = Config.CurrentValue;
			var prefix = localizer["k4.general.prefix"];
			var separator = localizer["k4.chat.separator"];
			var seasonName = _seasonService.CurrentConfig?.SeasonName ?? $"Season {_seasonService.CurrentSeasonId}";
			var remaining = _seasonService.SeasonEnd - DateTime.UtcNow;
			var daysLeft = Math.Max(0, (int)remaining.TotalDays);

			player.Player.SendChat($"{prefix} [lime]{localizer["k4.menu.info"]}:");
			player.Player.SendChat($" {localizer["k4.chat.season_dates", _seasonService.SeasonStart.ToString(config.General.DateFormat), _seasonService.SeasonEnd.ToString(config.General.DateFormat)]}");
			player.Player.SendChat($" {localizer["k4.chat.season_remaining", daysLeft]}");

			// Commands
			player.Player.SendChat($" [grey]----------------------------------------");
			player.Player.SendChat($" {localizer["k4.chat.commands_header"]}");
			player.Player.SendChat($" {localizer["k4.chat.cmd_season", config.Commands.Season.Command]}");
			if (config.Prestige.Enabled)
				player.Player.SendChat($" {localizer["k4.chat.cmd_prestige", config.Commands.Prestige.Command]}");
			player.Player.SendChat($" {localizer["k4.chat.cmd_reroll", config.Commands.RerollMission.Command]}");

			player.Player.SendChat($"{separator}");
		});
	}

	private void ShowProfile(SeasonPlayer player, ILocalizer localizer)
	{
		Core.Scheduler.NextWorldUpdate(() =>
		{
			if (!player.IsValid)
				return;

			var config = Config.CurrentValue;
			var prefix = localizer["k4.general.prefix"];
			var separator = localizer["k4.chat.separator"];
			var level = player.GetLevel(config);
			var currentXp = player.Experience;
			var nextXp = SeasonPlayer.GetRequiredExperience(config, level + 1);
			var multiplier = player.GetCurrentMultiplier(config, _toplistService.Toplist);

			player.Player.SendChat($"{prefix} [lime]{localizer["k4.menu.profile"]}:");
			player.Player.SendChat($" {localizer["k4.chat.profile_level", level, player.MaxLevel(config)]}");
			player.Player.SendChat($" {localizer["k4.chat.profile_xp", currentXp, nextXp]}");
			player.Player.SendChat($" {localizer["k4.chat.profile_multiplier", multiplier.ToString("F2")]}");

			if (config.Streak.Enabled)
				player.Player.SendChat($" {localizer["k4.chat.profile_streak", player.Streak]}");

			if (config.Prestige.Enabled)
				player.Player.SendChat($" {localizer["k4.chat.profile_prestige", player.Prestige, config.Prestige.LevelCap]}");

			if (player.HasBattlePass)
				player.Player.SendChat($" {localizer["k4.chat.profile_bp_active"]}");
			else
				player.Player.SendChat($" {localizer["k4.chat.profile_bp_inactive"]}");

			player.Player.SendChat($"{separator}");
		});
	}

	private void ShowMissionsMenu(SeasonPlayer player, ILocalizer localizer)
	{
		var builder = Core.MenusAPI.CreateBuilder()
			.Design.SetMenuTitle(localizer["k4.menu.missions"])
			.Design.SetMenuTitleVisible(true)
			.Design.SetMenuFooterVisible(true)
			.Design.SetGlobalScrollStyle(MenuOptionScrollStyle.LinearScroll);

		var personalBtn = new ButtonMenuOption($"<font color='#FF9800'>{localizer["k4.menu.missions_personal"]}</font>");
		personalBtn.Click += (_, _) =>
		{
			ShowPersonalMissions(player, localizer);
			return ValueTask.CompletedTask;
		};
		builder.AddOption(personalBtn);

		var communityBtn = new ButtonMenuOption($"<font color='#4CAF50'>{localizer["k4.menu.missions_community"]}</font>");
		communityBtn.Click += (_, _) =>
		{
			ShowCommunityMissions(player, localizer);
			return ValueTask.CompletedTask;
		};
		builder.AddOption(communityBtn);

		var menu = builder.Build();
		Core.MenusAPI.OpenMenuForPlayer(player.Player, menu);
	}

	private void ShowPersonalMissions(SeasonPlayer player, ILocalizer localizer)
	{
		Core.Scheduler.NextWorldUpdate(() =>
		{
			if (!player.IsValid)
				return;

			var config = Config.CurrentValue;
			var prefix = localizer["k4.general.prefix"];
			var separator = localizer["k4.chat.separator"];

			// Personal missions count
			var completedCount = player.PersonalMissions.Count(m => m.Completed);
			player.Player.SendChat($"{prefix} [lime]{localizer["k4.menu.missions_personal"]}: [grey]([yellow]{completedCount}[silver]/[lime]{player.PersonalMissions.Count}[grey] completed)");

			// Player limit warning
			if (_playerManager.ActivePlayerCount < config.Mission.MinPlayerCount)
			{
				player.Player.SendChat($" [lightred]⚠ {localizer["k4.chat.player_limit", config.Mission.MinPlayerCount]}");
			}

			// Reset time info
			var resetInfo = GetResetTimeInfo(player, localizer);
			if (!string.IsNullOrEmpty(resetInfo))
			{
				player.Player.SendChat($" [lightblue]⏱ [silver]Reset in: [white]{resetInfo}");
			}

			if (player.PersonalMissions.Count == 0)
			{
				player.Player.SendChat($" {localizer["k4.chat.no_missions"]}");
			}
			else
			{
				var counter = 1;
				foreach (var mission in player.PersonalMissions)
				{
					string statusText;
					string textColor;
					if (mission.Completed)
					{
						statusText = "[lime]✔";
						textColor = "lime";
					}
					else
					{
						statusText = $"[yellow]{mission.Progress}[silver]/[lime]{mission.AmountToComplete}";
						textColor = "white";
					}

					player.Player.SendChat($" [grey]» [{textColor}]#{counter} [white]{mission.Name} [grey]({statusText}[grey])");
					counter++;
				}

				// Locked Battle Pass slots
				var totalSlots = config.Mission.DailyMissionCount +
					(player.HasBattlePass ? config.BattlePass.DailyMissionCount : 0);
				var maxSlots = config.Mission.DailyMissionCount + config.BattlePass.DailyMissionCount;

				if (!player.HasBattlePass && maxSlots > totalSlots)
				{
					for (var i = counter; i <= maxSlots; i++)
					{
						player.Player.SendChat($" [grey]» 🔒 #{i} - {localizer["k4.chat.bp_locked"]}");
					}
				}
			}

			player.Player.SendChat($"{separator}");
		});
	}

	private void ShowCommunityMissions(SeasonPlayer player, ILocalizer localizer)
	{
		Core.Scheduler.NextWorldUpdate(() =>
		{
			if (!player.IsValid)
				return;

			var config = Config.CurrentValue;
			var prefix = localizer["k4.general.prefix"];
			var separator = localizer["k4.chat.separator"];

			// Community missions count
			var communityCompleted = _missionService.ActiveCommunityMissions.Count(m => m.Completed);
			var communityTotal = _missionService.ActiveCommunityMissions.Count;
			player.Player.SendChat($"{prefix} [lime]{localizer["k4.menu.missions_community"]}: [grey]([yellow]{communityCompleted}[silver]/[lime]{communityTotal}[grey] completed)");

			// Player limit warning
			if (_playerManager.ActivePlayerCount < config.Mission.MinPlayerCount)
			{
				player.Player.SendChat($" [lightred]⚠ {localizer["k4.chat.player_limit", config.Mission.MinPlayerCount]}");
			}

			if (_missionService.ActiveCommunityMissions.Count == 0)
			{
				player.Player.SendChat($" {localizer["k4.chat.no_missions"]}");
			}
			else
			{

				foreach (var cm in _missionService.ActiveCommunityMissions)
				{
					var status = cm.Completed ? "[lime]✔" : $"[yellow]{cm.Progress}[silver]/[lime]{cm.AmountToComplete}";
					var color = cm.Completed ? "lime" : "orange";
					player.Player.SendChat($" [grey]» [{color}]{cm.Name} [grey]({status}[grey])");
				}
			}

			player.Player.SendChat($"{separator}");
		});
	}

	private static void ShowBattlePassInfo(SeasonPlayer player, ILocalizer localizer)
	{
		Core.Scheduler.NextWorldUpdate(() =>
		{
			if (!player.IsValid)
				return;

			var config = Config.CurrentValue;
			var prefix = localizer["k4.general.prefix"];
			var separator = localizer["k4.chat.separator"];

			player.Player.SendChat($"{prefix} [lime]{localizer["k4.menu.battlepass"]}:");
			player.Player.SendChat($" {localizer["k4.chat.bp_multiplier", config.BattlePass.Multiplier]}");
			player.Player.SendChat($" {localizer["k4.chat.bp_level_cap", config.BattlePass.LevelCap]}");
			player.Player.SendChat($" {localizer["k4.chat.bp_extra_missions", config.BattlePass.DailyMissionCount]}");
			player.Player.SendChat($" {localizer["k4.chat.bp_extra_rerolls", config.BattlePass.MissionRerollCount]}");
			player.Player.SendChat($" [grey]----------------------------------------");
			if (player.HasBattlePass)
				player.Player.SendChat($" {localizer["k4.chat.bp_status_active"]}");
			else
				player.Player.SendChat($" {localizer["k4.chat.bp_status_inactive"]}");

			player.Player.SendChat($"{separator}");
		});
	}

	private void ShowLevelList(SeasonPlayer player, ILocalizer localizer)
	{
		var config = Config.CurrentValue;
		var seasonConfig = _seasonService.CurrentConfig;

		if (seasonConfig?.Rewards == null)
			return;

		var builder = Core.MenusAPI.CreateBuilder()
			.Design.SetMenuTitle(localizer["k4.menu.levels"])
			.Design.SetMenuTitleVisible(true)
			.Design.SetMenuFooterVisible(true)
			.Design.SetGlobalScrollStyle(MenuOptionScrollStyle.LinearScroll);

		var currentLevel = player.GetLevel(config);

		foreach (var (level, reward) in seasonConfig.Rewards.OrderBy(r => r.Key))
		{
			var claimed = player.ClaimedBattlePassLevels.Contains(level);
			var locked = reward.BattlePassOnly && !player.HasBattlePass;
			var reachable = level <= currentLevel;

			string color;
			string statusIcon;

			if (claimed) { color = "#4CAF50"; statusIcon = "✓"; }
			else if (locked) { color = "#666666"; statusIcon = "🔒"; }
			else if (reachable) { color = "#FFD700"; statusIcon = "★"; }
			else { color = "#AAAAAA"; statusIcon = "○"; }

			var bpTag = reward.BattlePassOnly ? " <font color='#E91E63'>[BP]</font>" : "";

			var btn = new ButtonMenuOption(
				$"<font color='{color}'>{statusIcon} Lv.{level}{bpTag}</font>");

			var capturedLevel = level;
			var capturedReward = reward;

			btn.Click += (_, _) =>
			{
				ShowLevelDetails(player, localizer, capturedLevel, capturedReward);
				return ValueTask.CompletedTask;
			};
			builder.AddOption(btn);
		}

		Core.MenusAPI.OpenMenuForPlayer(player.Player, builder.Build());
	}

	private static void ShowLevelDetails(SeasonPlayer player, ILocalizer localizer, int level, SeasonReward reward)
	{
		Core.Scheduler.NextWorldUpdate(() =>
		{
			if (!player.IsValid)
				return;

			var config = Config.CurrentValue;
			var prefix = localizer["k4.general.prefix"];
			var separator = localizer["k4.chat.separator"];
			var requiredXp = SeasonPlayer.GetRequiredExperience(config, level);
			var claimed = player.ClaimedBattlePassLevels.Contains(level);
			var locked = reward.BattlePassOnly && !player.HasBattlePass;
			var currentLevel = player.GetLevel(config);
			var reachable = level <= currentLevel;

			player.Player.SendChat($"{prefix} [lime]Level {level}:");
			player.Player.SendChat($" {localizer["k4.chat.level_name", reward.Name]}");
			// Required XP with status inline
			var xpStatus = "";
			if (!claimed && !locked && !reachable)
				xpStatus = " [grey](Not Reached Yet)";

			player.Player.SendChat($" [grey]> [silver]Required XP: [white]{requiredXp}{xpStatus}");

			// Status
			if (claimed)
				player.Player.SendChat($" [grey]> [lime]✔ Reward Claimed");
			else if (locked)
				player.Player.SendChat($" [grey]> [gold]🎫 Battle Pass Required");
			else if (reachable)
				player.Player.SendChat($" [grey]> [yellow]★ Available to Claim");

			// Rewards
			if (reward.Permissions.Count > 0)
			{
				player.Player.SendChat($" [lightblue]Permissions:");
				foreach (var perm in reward.Permissions.Take(3))
					player.Player.SendChat($"   [grey]• [white]{perm}");
				if (reward.Permissions.Count > 3)
					player.Player.SendChat($"   [grey]... and {reward.Permissions.Count - 3} more");
			}

			player.Player.SendChat($"{separator}");
		});
	}

	private static string GetResetTimeInfo(SeasonPlayer player, ILocalizer localizer)
	{
		if (player.PersonalMissions.Count == 0)
			return string.Empty;

		var oldestMission = player.PersonalMissions.OrderBy(m => m.CreatedAt).FirstOrDefault();
		if (oldestMission == null)
			return string.Empty;

		var nextReset = oldestMission.CreatedAt.Date.AddDays(1);
		var remaining = nextReset - DateTime.UtcNow;

		if (remaining.TotalHours < 0)
			return localizer["k4.chat.reset_pending"].ToString();

		var hours = (int)remaining.TotalHours;
		var minutes = remaining.Minutes;

		return localizer["k4.chat.reset_time", hours, minutes].ToString();
	}

	private void ShowToplist(SeasonPlayer player, ILocalizer localizer)
	{
		var config = Config.CurrentValue;

		if (!config.Toplist.Enabled)
			return;

		var builder = Core.MenusAPI.CreateBuilder()
			.Design.SetMenuTitle(localizer["k4.menu.toplist"])
			.Design.SetMenuTitleVisible(true)
			.Design.SetMenuFooterVisible(true)
			.Design.SetGlobalScrollStyle(MenuOptionScrollStyle.LinearScroll);

		var toplist = _toplistService.Toplist.Values.OrderBy(e => e.Rank).Take(10).ToList();

		foreach (var entry in toplist)
		{
			var color = entry.Rank switch
			{
				1 => "#FFD700",
				2 => "#C0C0C0",
				3 => "#CD7F32",
				_ => "#FFFFFF"
			};

			var btn = new ButtonMenuOption($"<font color='{color}'>#{entry.Rank} {entry.UserName} - {entry.Experience:N0} XP</font>");
			btn.Click += (_, _) => ValueTask.CompletedTask;
			builder.AddOption(btn);
		}

		Core.MenusAPI.OpenMenuForPlayer(player.Player, builder.Build());

		// Send player's rank to chat
		Task.Run(async () =>
		{
			var (rank, total) = await _database.GetPlayerRankAsync(player.SteamId);

			Core.Scheduler.NextWorldUpdate(() =>
			{
				if (!player.IsValid)
					return;

				var prefix = localizer["k4.general.prefix"];
				player.Player.SendChat($"{prefix} {localizer["k4.chat.toplist_rank", rank, total, player.Experience]}");
			});
		});
	}
}

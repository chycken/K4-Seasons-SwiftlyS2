using Microsoft.Extensions.Logging;
using SwiftlyS2.Shared.Commands;

namespace K4Seasons;

public sealed partial class Plugin
{
	private void RegisterCommands()
	{
		var cmds = Config.CurrentValue.Commands;

		RegisterCommand(cmds.Season, OnSeasonCommand);
		RegisterCommand(cmds.Prestige, OnPrestigeCommand);
		RegisterCommand(cmds.GiveBattlePass, OnGiveBpCommand);
		RegisterCommand(cmds.RerollMission, OnRerollCommand);
		RegisterCommand(cmds.AbandonMission, OnAbandonCommand);
	}

	private static void RegisterCommand(CommandConfig cmd, ICommandService.CommandListener handler)
	{
		if (string.IsNullOrEmpty(cmd.Command))
			return;

		Core.Command.RegisterCommand(cmd.Command, handler);

		foreach (var alias in cmd.Aliases)
			Core.Command.RegisterCommandAlias(cmd.Command, alias);
	}

	private void OnSeasonCommand(ICommandContext ctx)
	{
		var player = ctx.Sender;

		if (player == null || !player.IsValid)
			return;

		var sp = _playerManager.GetPlayer(player);

		if (sp is not { IsLoaded: true })
			return;

		ShowMainMenu(sp);
	}

	private void OnPrestigeCommand(ICommandContext ctx)
	{
		var player = ctx.Sender;

		if (player == null || !player.IsValid)
			return;

		var sp = _playerManager.GetPlayer(player);

		if (sp is not { IsLoaded: true })
			return;

		var config = Config.CurrentValue;
		var localizer = Core.Translation.GetPlayerLocalizer(player);

		if (!config.Prestige.Enabled)
		{
			player.SendChat($"{localizer["k4.general.prefix"]} {localizer["k4.chat.prestige_disabled"]}");
			return;
		}

		if (sp.Prestige >= config.Prestige.LevelCap)
		{
			player.SendChat($"{localizer["k4.general.prefix"]} {localizer["k4.chat.prestige_max"]}");
			return;
		}

		var maxLevel = sp.MaxLevel(config);
		var currentLevel = sp.GetLevel(config);

		if (currentLevel < maxLevel)
		{
			player.SendChat($"{localizer["k4.general.prefix"]} {localizer["k4.chat.prestige_not_max", maxLevel]}");
			return;
		}

		_playerManager.Prestige(sp);
		player.SendChat($"{localizer["k4.general.prefix"]} {localizer["k4.chat.prestige_success", sp.Prestige]}");
	}

	private void OnGiveBpCommand(ICommandContext ctx)
	{
		var sender = ctx.Sender;
		var cmd = Config.CurrentValue.Commands.GiveBattlePass;

		if (sender != null && !string.IsNullOrEmpty(cmd.Permission) &&
			!Core.Permission.PlayerHasPermission(sender.SteamID, cmd.Permission))
		{
			var localizer = Core.Translation.GetPlayerLocalizer(sender);
			sender.SendChat($"{localizer["k4.general.prefix"]} {localizer["k4.chat.no_permission"]}");
			return;
		}

		if (ctx.Args.Length < 1)
		{
			if (sender != null)
			{
				var localizer = Core.Translation.GetPlayerLocalizer(sender);
				sender.SendChat($"{localizer["k4.general.prefix"]} {localizer["k4.chat.usage_givebp"]}");
			}
			return;
		}

		var targetArg = ctx.Args[0];
		SeasonPlayer? target = null;

		if (ulong.TryParse(targetArg, out var steamId))
		{
			target = _playerManager.GetPlayer(steamId);
		}
		else
		{
			target = _playerManager.AllPlayers.FirstOrDefault(p =>
				p.IsValid && p.UserName.Contains(targetArg, StringComparison.OrdinalIgnoreCase));
		}

		if (target == null)
		{
			if (sender != null)
			{
				var localizer = Core.Translation.GetPlayerLocalizer(sender);
				sender.SendChat($"{localizer["k4.general.prefix"]} {localizer["k4.chat.player_not_found"]}");
			}
			return;
		}

		if (target.HasBattlePass)
		{
			if (sender != null)
			{
				var localizer = Core.Translation.GetPlayerLocalizer(sender);
				sender.SendChat($"{localizer["k4.general.prefix"]} {localizer["k4.chat.already_has_bp"]}");
			}
			return;
		}

		_playerManager.ActivateBattlePass(target);

		var targetLocalizer = Core.Translation.GetPlayerLocalizer(target.Player);
		target.Player.SendChat($"{targetLocalizer["k4.general.prefix"]} {targetLocalizer["k4.chat.bp_activated"]}");

		Core.Logger.LogInformation("Battle Pass activated for {Name} ({SteamId}) by admin.",
			target.UserName, target.SteamId);
	}

	private void OnRerollCommand(ICommandContext ctx)
	{
		var player = ctx.Sender;

		if (player == null || !player.IsValid)
			return;

		var sp = _playerManager.GetPlayer(player);

		if (sp is not { IsLoaded: true })
			return;

		var localizer = Core.Translation.GetPlayerLocalizer(player);

		if (ctx.Args.Length < 1 || !int.TryParse(ctx.Args[0], out var missionNumber) || missionNumber < 1)
		{
			player.SendChat($"{localizer["k4.general.prefix"]} {localizer["k4.chat.usage_reroll"]}");
			return;
		}

		if (missionNumber > sp.PersonalMissions.Count)
		{
			player.SendChat($"{localizer["k4.general.prefix"]} {localizer["k4.chat.mission_not_found"]}");
			return;
		}

		var mission = sp.PersonalMissions[missionNumber - 1];

		if (mission.Completed)
		{
			player.SendChat($"{localizer["k4.general.prefix"]} {localizer["k4.chat.mission_already_completed"]}");
			return;
		}

		if (sp.Rerolls <= 0)
		{
			player.SendChat($"{localizer["k4.general.prefix"]} {localizer["k4.chat.no_rerolls"]}");
			return;
		}

		Task.Run(async () =>
		{
			var newMission = await _missionService.RerollMissionAsync(sp, mission);

			Core.Scheduler.NextWorldUpdate(() =>
			{
				if (!sp.IsValid)
					return;

				var loc = Core.Translation.GetPlayerLocalizer(sp.Player);

				if (newMission != null)
					sp.Player.SendChat($"{loc["k4.general.prefix"]} {loc["k4.chat.reroll_success", newMission.Name]}");
				else
					sp.Player.SendChat($"{loc["k4.general.prefix"]} {loc["k4.chat.reroll_failed"]}");
			});
		});
	}

	private void OnAbandonCommand(ICommandContext ctx)
	{
		var player = ctx.Sender;

		if (player == null || !player.IsValid)
			return;

		var sp = _playerManager.GetPlayer(player);

		if (sp is not { IsLoaded: true })
			return;

		var localizer = Core.Translation.GetPlayerLocalizer(player);

		if (ctx.Args.Length < 1 || !int.TryParse(ctx.Args[0], out var missionNumber) || missionNumber < 1)
		{
			if (sp.FrustratedMissionId.HasValue)
				player.SendChat($"{localizer["k4.general.prefix"]} {localizer["k4.chat.mission_abandon_command", 0, sp.FrustratedMissionId.Value]}");
			return;
		}

		// Check if this mission was offered by anti-frustration system
		if (!sp.FrustratedMissionId.HasValue || sp.FrustratedMissionId.Value != missionNumber)
		{
			player.SendChat($"{localizer["k4.general.prefix"]} {localizer["k4.chat.mission_not_frustrated"]}");
			return;
		}

		if (missionNumber > sp.PersonalMissions.Count)
		{
			player.SendChat($"{localizer["k4.general.prefix"]} {localizer["k4.chat.mission_not_found"]}");
			return;
		}

		var mission = sp.PersonalMissions[missionNumber - 1];

		if (mission.Completed)
		{
			player.SendChat($"{localizer["k4.general.prefix"]} {localizer["k4.chat.mission_already_completed"]}");
			return;
		}

		Task.Run(async () =>
		{
			var partialXp = (int)(mission.RewardExperience * ((float)mission.Progress / mission.AmountToComplete));
			await _missionService.AbandonMission(sp, mission, _playerManager);

			Core.Scheduler.NextWorldUpdate(() =>
			{
				if (!sp.IsValid)
					return;

				sp.FrustratedMissionId = null; // Clear frustrated mission after abandoning

				var loc = Core.Translation.GetPlayerLocalizer(sp.Player);
				sp.Player.SendChat($"{loc["k4.general.prefix"]} {loc["k4.chat.mission_abandon_success", partialXp]}");
			});
		});
	}
}

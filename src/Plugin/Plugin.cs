using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Plugins;

namespace K4Seasons;

[PluginMetadata(
	Id = "k4.seasons",
	Version = "1.0.0",
	Name = "K4 - Seasons",
	Author = "K4ryuu",
	Description = "A comprehensive battle pass and season system for Counter-Strike 2 using SwiftlyS2 framework.")]
public sealed partial class Plugin(ISwiftlyCore core) : BasePlugin(core)
{
	public static new ISwiftlyCore Core { get; private set; } = null!;
	public static IOptionsMonitor<PluginConfig> Config { get; private set; } = null!;

	private DatabaseService _database = null!;
	private SeasonService _seasonService = null!;
	private MissionLoader _missionLoader = null!;
	private MissionService _missionService = null!;
	private PlayerManager _playerManager = null!;
	private ExperienceService _experienceService = null!;
	private ToplistService _toplistService = null!;

	private readonly Dictionary<string, HashSet<string>> _registeredEvents = [];
	private int _lastRoundWinner;
	private int _tickCount;

	public override void Load(bool hotReload)
	{
		Core = base.Core;

		LoadConfiguration();
		InitializeServices(hotReload);
		RegisterCommands();
		RegisterEventHandlers();
		_experienceService.RegisterXpEvents();
		RegisterMissionEvents();
	}

	public override void Unload()
	{
		Task.Run(async () => await _playerManager.SaveAllPlayersAsync()).Wait();
		_playerManager.Clear();
	}

	private static void LoadConfiguration()
	{
		const string configFileName = "config.json";
		const string configSection = "K4Seasons";

		Core.Configuration
			.InitializeJsonWithModel<PluginConfig>(configFileName, configSection)
			.Configure(builder =>
			{
				builder.AddJsonFile(configFileName, optional: false, reloadOnChange: true);
			});

		ServiceCollection services = new();
		services.AddSwiftly(Core)
			.AddOptionsWithValidateOnStart<PluginConfig>()
			.BindConfiguration(configSection);

		var provider = services.BuildServiceProvider();
		Config = provider.GetRequiredService<IOptionsMonitor<PluginConfig>>();
	}

	private void InitializeServices(bool hotReload = false)
	{
		var config = Config.CurrentValue;

		_database = new DatabaseService(config.Database.Connection);
		_missionLoader = new MissionLoader();
		_missionLoader.LoadFromFile(Core.PluginPath);

		_toplistService = new ToplistService(_database);
		_missionService = new MissionService(_database, _missionLoader);
		_seasonService = new SeasonService(_database);
		_playerManager = new PlayerManager(_database, _missionService, _seasonService, _toplistService);
		_experienceService = new ExperienceService(_playerManager);

		_missionService.OnPersonalMissionCompleted += HandlePersonalMissionCompleted;
		_missionService.OnCommunityMissionCompleted += HandleCommunityMissionCompleted;

		Task.Run(async () =>
		{
			await _database.InitializeAsync();
			await _seasonService.InitializeAsync();
			await _missionService.LoadCommunityMissionsAsync();

			if (config.Toplist.Enabled)
				await _toplistService.UpdateAsync();

			// Hot reload: Load players AFTER database is initialized
			if (hotReload)
			{
				Core.Scheduler.NextWorldUpdate(() =>
				{
					var allPlayers = Core.PlayerManager.GetAllPlayers();

					foreach (var player in allPlayers)
					{
						if (player.IsValid && !player.IsFakeClient)
						{
							_playerManager.GetOrCreatePlayer(player);
						}
					}
				});
			}
		});

		// Check for battle pass updates every 5 seconds with optimized single query
		Core.Scheduler.RepeatBySeconds(5f, () =>
		{
			Task.Run(async () => await _playerManager.CheckBattlePassUpdatesAsync());
		});

		Core.Scheduler.RepeatBySeconds(60f, () =>
		{
			_tickCount++;
			var cfg = Config.CurrentValue;

			_playerManager.ProcessActiveTime();

			if (cfg.Mission.MinPlayerCount <= _playerManager.ActivePlayerCount)
				_missionService.ProcessPlayTime(_playerManager.AllPlayers);

			CheckPrestigeReminders();
			CheckAntiFrustration();

			if (cfg.Toplist.Enabled && _tickCount % 5 == 0)
				Task.Run(async () => await _toplistService.UpdateAsync());

			var progressMinutes = cfg.Experience.ShowProgressOnMinutes;
			if (progressMinutes > 0 && _tickCount % progressMinutes == 0)
			{
				foreach (var player in _playerManager.AllPlayers.Where(p => p.IsValid && p.IsLoaded))
					ShowProgressChat(player);
			}

			if (_tickCount % 720 == 0)
			{
				Task.Run(async () =>
				{
					await _database.PurgeOldDataAsync(cfg.Database.PurgeDays);

					if (DateTime.UtcNow > _seasonService.SeasonEnd)
					{
						await _seasonService.TransitionSeasonAsync();
					}

					if (_missionService.ShouldResetCommunityMissions())
						await _missionService.AssignCommunityMissionsAsync();
				});
			}
		});
	}

	private void HandlePersonalMissionCompleted(SeasonPlayer player, DbMission mission)
	{
		Core.Scheduler.NextWorldUpdate(() =>
		{
			if (!player.IsValid)
				return;

			_playerManager.AddExperience(player, mission.RewardExperience);

			var localizer = Core.Translation.GetPlayerLocalizer(player.Player);
			player.Player.SendChat($"{localizer["k4.general.prefix"]} {localizer["k4.chat.mission_complete", mission.Name, mission.RewardExperience]}");
		});
	}

	private void HandleCommunityMissionCompleted(DbCommunityMission mission)
	{
		Core.Scheduler.NextWorldUpdate(() =>
		{
			foreach (var player in _playerManager.AllPlayers.Where(p => p.IsValid && p.IsLoaded))
			{
				_playerManager.AddExperience(player, mission.RewardExperience);

				var localizer = Core.Translation.GetPlayerLocalizer(player.Player);
				player.Player.SendChat($"{localizer["k4.general.prefix"]} {localizer["k4.chat.community_complete", mission.Name, mission.RewardExperience]}");
			}
		});
	}

	private void ShowProgressChat(SeasonPlayer player)
	{
		if (!player.IsValid)
			return;

		var config = Config.CurrentValue;
		var localizer = Core.Translation.GetPlayerLocalizer(player.Player);
		var level = player.GetLevel(config);
		var currentXp = player.Experience;
		var nextLevelXp = SeasonPlayer.GetRequiredExperience(config, level + 1);
		var multiplier = player.GetCurrentMultiplier(config, _toplistService.Toplist);

		player.Player.SendChat($"{localizer["k4.general.prefix"]} {localizer["k4.chat.progress", level, currentXp, nextLevelXp, multiplier.ToString("F2")]}");
	}


	private void CheckPrestigeReminders()
	{
		var config = Config.CurrentValue;

		if (!config.Prestige.Enabled || config.Prestige.ReminderMinuteInterval <= 0)
			return;

		foreach (var player in _playerManager.AllPlayers.Where(p => p.IsValid && p.IsLoaded))
		{
			if (player.Prestige >= config.Prestige.LevelCap)
				continue;

			var maxLevel = player.MaxLevel(config);
			var currentLevel = player.GetLevel(config);

			if (currentLevel < maxLevel)
				continue;

			if ((DateTime.UtcNow - player.LastPrestigeReminder).TotalMinutes < config.Prestige.ReminderMinuteInterval)
				continue;

			player.LastPrestigeReminder = DateTime.UtcNow;

			var localizer = Core.Translation.GetPlayerLocalizer(player.Player);
			player.Player.SendChat($"{localizer["k4.general.prefix"]} {localizer["k4.chat.prestige_available"]}");
		}
	}

	private void CheckAntiFrustration()
	{
		var config = Config.CurrentValue;

		if (!config.AntiFrustration.Enabled)
			return;

		foreach (var player in _playerManager.AllPlayers.Where(p => p.IsValid && p.IsLoaded))
		{
			if (player.ActiveTime < config.AntiFrustration.MinuteInterval)
				continue;

			if ((DateTime.UtcNow - player.LastFrustrationCheck).TotalMinutes < config.AntiFrustration.BetweenDelay)
				continue;

			var incomplete = player.PersonalMissions.Where(m => !m.Completed && m.Progress > 0).ToList();

			if (incomplete.Count == 0)
				continue;

			var least = incomplete.OrderBy(m => m.AmountToComplete > 0 ? (float)m.Progress / m.AmountToComplete : 1f).First();
			var percent = least.AmountToComplete > 0 ? (float)least.Progress / least.AmountToComplete : 1f;

			if (percent >= 1f)
				continue;

			var missionIndex = player.PersonalMissions.IndexOf(least) + 1;
			player.FrustratedMissionId = missionIndex;
			player.LastFrustrationCheck = DateTime.UtcNow;

			var localizer = Core.Translation.GetPlayerLocalizer(player.Player);
			var partialXp = (int)(least.RewardExperience * percent);
			var percentDisplay = (percent * 100).ToString("F1");

			player.Player.SendChat($"{localizer["k4.general.prefix"]} {localizer["k4.chat.mission_reminder", least.Name, least.Progress, least.AmountToComplete, percentDisplay]}");
			player.Player.SendChat($"{localizer["k4.general.prefix"]} {localizer["k4.chat.mission_abandon_command", partialXp, missionIndex]}");
		}
	}
}

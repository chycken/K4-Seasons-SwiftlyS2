using System.Reflection;
using Microsoft.Extensions.Logging;
using SwiftlyS2.Shared.Events;
using SwiftlyS2.Shared.GameEventDefinitions;
using SwiftlyS2.Shared.GameEvents;
using SwiftlyS2.Shared.Misc;
using SwiftlyS2.Shared.Players;

namespace K4Seasons;

public sealed partial class Plugin
{
	private void RegisterEventHandlers()
	{
		Core.GameEvent.HookPost<EventPlayerActivate>(OnPlayerActivate);
		Core.GameEvent.HookPost<EventPlayerDisconnect>(OnPlayerDisconnect);
		Core.GameEvent.HookPost<EventRoundEnd>(OnRoundEnd);
		Core.GameEvent.HookPost<EventCsWinPanelMatch>(OnGameEnd);
		Core.Event.OnMapLoad += OnMapLoad;
	}

	private HookResult OnPlayerActivate(EventPlayerActivate @event)
	{
		var player = Core.PlayerManager.GetPlayer(@event.UserId);

		if (player?.IsValid != true || player.IsFakeClient)
			return HookResult.Continue;

		_playerManager.GetOrCreatePlayer(player);
		return HookResult.Continue;
	}

	private HookResult OnPlayerDisconnect(EventPlayerDisconnect @event)
	{
		var player = Core.PlayerManager.GetPlayer(@event.UserId);

		if (player != null)
			_playerManager.RemovePlayer(player.SteamID);

		return HookResult.Continue;
	}

	private HookResult OnRoundEnd(EventRoundEnd @event)
	{
		var winner = @event.Winner;
		_lastRoundWinner = winner;

		if (winner > (int)Team.Spectator)
			_experienceService.RewardRoundWin(winner);

		if (Config.CurrentValue.General.RoundEndSave)
			Task.Run(async () => await _playerManager.SaveAllPlayersAsync());

		if (Config.CurrentValue.Experience.ShowProgressOnDeath)
		{
			foreach (var player in _playerManager.AllPlayers.Where(p => p.IsValid && p.IsLoaded))
				ShowProgressChat(player);
		}

		return HookResult.Continue;
	}

	private HookResult OnGameEnd(EventCsWinPanelMatch @event)
	{
		if (_lastRoundWinner > (int)Team.Spectator)
			_experienceService.RewardGameWin(_lastRoundWinner);

		Task.Run(async () => await _playerManager.SaveAllPlayersAsync());
		return HookResult.Continue;
	}

	private void OnMapLoad(IOnMapLoadEvent @event)
	{
		_missionService.CurrentMap = @event.MapName;

		Core.Scheduler.DelayBySeconds(0.1f, () =>
		{
			Task.Run(async () =>
			{
				if (_missionService.ShouldResetCommunityMissions())
					await _missionService.AssignCommunityMissionsAsync();
			});
		});
	}

	private void RegisterMissionEvents()
	{
		var missions = _missionLoader.GetAllMissions();

		foreach (var mission in missions)
		{
			if (mission.Event == "PlayTime")
				continue;

			if (_registeredEvents.TryGetValue(mission.Event, out var targets))
			{
				targets.Add(mission.Target);
				continue;
			}

			if (RegisterEventForMission(mission.Event))
				_registeredEvents[mission.Event] = [mission.Target];
		}

		Core.Logger.LogInformation("Registered {Count} mission event types.", _registeredEvents.Count);
	}

	private bool RegisterEventForMission(string eventName)
	{
		var eventType = AppDomain.CurrentDomain.GetAssemblies()
			.Select(a => a.GetType($"SwiftlyS2.Shared.GameEventDefinitions.{eventName}"))
			.FirstOrDefault(t => t != null);

		if (eventType == null)
		{
			Core.Logger.LogWarning("Event type {Event} not found.", eventName);
			return false;
		}

		var gameEventInterface = eventType.GetInterfaces()
			.FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IGameEvent<>));

		if (gameEventInterface == null)
		{
			Core.Logger.LogWarning("Event type {Event} does not implement IGameEvent<T>.", eventName);
			return false;
		}

		try
		{
			var hookPostMethod = typeof(IGameEventService).GetMethod("HookPost");

			if (hookPostMethod == null)
				return false;

			var genericHookPost = hookPostMethod.MakeGenericMethod(eventType);
			var handlerMethod = GetType().GetMethod(nameof(OnGenericMissionEvent), BindingFlags.NonPublic | BindingFlags.Instance);

			if (handlerMethod == null)
				return false;

			var genericHandler = handlerMethod.MakeGenericMethod(eventType);
			var delegateType = typeof(IGameEventService.GameEventHandler<>).MakeGenericType(eventType);
			var handlerDelegate = Delegate.CreateDelegate(delegateType, this, genericHandler);

			genericHookPost.Invoke(Core.GameEvent, [handlerDelegate]);
			return true;
		}
		catch (Exception ex)
		{
			Core.Logger.LogError(ex, "Failed to register event {Event}.", eventName);
			return false;
		}
	}

	private HookResult OnGenericMissionEvent<T>(T @event) where T : IGameEvent<T>
	{
		var eventType = typeof(T).Name;

		if (!Config.CurrentValue.Mission.RecordWarmup)
		{
			var gameRules = Core.EntitySystem.GetGameRules();

			if (gameRules?.WarmupPeriod == true)
				return HookResult.Continue;
		}

		if (eventType == "EventRoundEnd")
		{
			HandleRoundEndMissions(@event);
			return HookResult.Continue;
		}

		if (!_registeredEvents.TryGetValue(eventType, out var targets))
			return HookResult.Continue;

		var properties = ExtractProperties(@event);

		foreach (var target in targets)
		{
			var player = ResolvePlayer(@event, target);

			if (player?.IsValid != true || player.IsFakeClient)
				continue;

			if (Config.CurrentValue.Mission.EventDebugLogs)
			{
				foreach (var (key, val) in properties)
					Core.Logger.LogInformation("[{Event}] {Key}: {Value}", eventType, key, val);
			}

			var seasonPlayer = _playerManager.GetPlayer(player);

			if (seasonPlayer is { IsLoaded: true })
				_playerManager.MissionService.ProcessEvent(seasonPlayer, eventType, target, properties);
		}

		return HookResult.Continue;
	}

	private void HandleRoundEndMissions<T>(T @event)
	{
		var winnerProp = typeof(T).GetProperty("Winner");

		if (winnerProp == null)
			return;

		var winner = Convert.ToInt32(winnerProp.GetValue(@event) ?? 0);

		if (winner <= (int)Team.Spectator)
			return;

		foreach (var sp in _playerManager.AllPlayers.Where(p => p.IsValid && p.IsLoaded))
		{
			var playerTeam = (int)(sp.Player.Controller?.Team ?? Team.None);

			if (playerTeam <= (int)Team.Spectator)
				continue;

			var target = playerTeam == winner ? "winner" : "loser";
			_playerManager.MissionService.ProcessEvent(sp, "EventRoundEnd", target, null);
		}
	}

	private static IPlayer? ResolvePlayer<T>(T @event, string target)
	{
		var accessorProp = typeof(T).GetProperty("Accessor")
			?? typeof(T).GetInterfaces()
				.SelectMany(i => new[] { i }.Concat(i.GetInterfaces()))
				.Select(i => i.GetProperty("Accessor"))
				.FirstOrDefault(p => p != null);

		if (accessorProp?.GetValue(@event) is IGameEventAccessor accessor)
		{
			var player = accessor.GetPlayer(target.ToLower());

			if (player != null)
				return player;
		}

		var playerProp = typeof(T).GetProperty($"{target}Player");
		return playerProp?.GetValue(@event) as IPlayer;
	}

	private static Dictionary<string, object?> ExtractProperties<T>(T @event)
	{
		var props = new Dictionary<string, object?>();

		foreach (var prop in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
		{
			if (prop.CanRead)
				props[prop.Name] = prop.GetValue(@event);
		}

		return props;
	}
}

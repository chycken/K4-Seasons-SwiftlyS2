using SwiftlyS2.Shared.GameEventDefinitions;
using SwiftlyS2.Shared.GameEvents;
using SwiftlyS2.Shared.Misc;
using SwiftlyS2.Shared.Players;

namespace K4Seasons;

public sealed partial class Plugin
{
	public sealed class ExperienceService(PlayerManager playerManager)
	{
		private readonly PlayerManager _playerManager = playerManager;

		public void RegisterXpEvents()
		{
			if (!Config.CurrentValue.Experience.EventsEnabled)
				return;

			Core.GameEvent.HookPost<EventRoundMvp>(OnMvp);
			Core.GameEvent.HookPost<EventHostageRescued>(OnHostageRescued);
			Core.GameEvent.HookPost<EventBombDefused>(OnBombDefused);
			Core.GameEvent.HookPost<EventBombPlanted>(OnBombPlanted);
			Core.GameEvent.HookPost<EventPlayerDeath>(OnPlayerDeath);
		}

		private HookResult OnMvp(EventRoundMvp @event)
		{
			RewardPlayer(@event.UserId, Config.CurrentValue.Experience.MvpReward);
			return HookResult.Continue;
		}

		private HookResult OnHostageRescued(EventHostageRescued @event)
		{
			RewardPlayer(@event.UserId, Config.CurrentValue.Experience.HostageRescueReward);
			return HookResult.Continue;
		}

		private HookResult OnBombDefused(EventBombDefused @event)
		{
			RewardPlayer(@event.UserId, Config.CurrentValue.Experience.BombDefuseReward);
			return HookResult.Continue;
		}

		private HookResult OnBombPlanted(EventBombPlanted @event)
		{
			RewardPlayer(@event.UserId, Config.CurrentValue.Experience.BombPlantReward);
			return HookResult.Continue;
		}

		private HookResult OnPlayerDeath(EventPlayerDeath @event)
		{
			var attacker = Core.PlayerManager.GetPlayer(@event.Attacker);

			if (attacker?.IsValid == true && !attacker.IsFakeClient)
				RewardPlayer(@event.Attacker, Config.CurrentValue.Experience.KillReward);

			return HookResult.Continue;
		}

		private void RewardPlayer(int userId, int baseReward)
		{
			if (baseReward <= 0)
				return;

			var config = Config.CurrentValue;

			if (!config.Experience.WarmupExperience)
			{
				var gameRules = Core.EntitySystem.GetGameRules();

				if (gameRules?.WarmupPeriod == true)
					return;
			}

			if (config.Experience.MinPlayerCount > _playerManager.ActivePlayerCount)
				return;

			var player = Core.PlayerManager.GetPlayer(userId);

			if (player?.IsValid != true || player.IsFakeClient)
				return;

			var seasonPlayer = _playerManager.GetPlayer(player);

			if (seasonPlayer is { IsLoaded: true })
				_playerManager.AddExperience(seasonPlayer, baseReward);
		}

		public void RewardRoundWin(int winnerTeam)
		{
			var config = Config.CurrentValue;

			if (config.Experience.RoundWinReward <= 0)
				return;

			if (config.Experience.MinPlayerCount > _playerManager.ActivePlayerCount)
				return;

			foreach (var sp in _playerManager.AllPlayers.Where(p => p.IsValid && p.IsLoaded))
			{
				var playerTeam = (int)(sp.Player.Controller?.Team ?? Team.None);

				if (playerTeam == winnerTeam)
					_playerManager.AddExperience(sp, config.Experience.RoundWinReward);
			}
		}

		public void RewardGameWin(int winnerTeam)
		{
			var config = Config.CurrentValue;

			if (config.Experience.GameWinReward <= 0)
				return;

			if (config.Experience.MinPlayerCount > _playerManager.ActivePlayerCount)
				return;

			foreach (var sp in _playerManager.AllPlayers.Where(p => p.IsValid && p.IsLoaded))
			{
				var playerTeam = (int)(sp.Player.Controller?.Team ?? Team.None);

				if (playerTeam == winnerTeam)
					_playerManager.AddExperience(sp, config.Experience.GameWinReward);
			}
		}
	}
}

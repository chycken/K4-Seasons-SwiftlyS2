using Microsoft.Extensions.Logging;

namespace K4Seasons;

public sealed partial class Plugin
{
	public sealed class MissionService(DatabaseService database, MissionLoader loader)
	{
		private readonly DatabaseService _database = database;
		private readonly MissionLoader _loader = loader;

		public List<DbCommunityMission> ActiveCommunityMissions { get; } = [];
		public string CurrentMap { get; set; } = string.Empty;

		public event Action<SeasonPlayer, DbMission>? OnPersonalMissionCompleted;
		public event Action<DbCommunityMission>? OnCommunityMissionCompleted;

		public async Task LoadCommunityMissionsAsync()
		{
			ActiveCommunityMissions.Clear();
			var missions = await _database.GetCommunityMissionsAsync();
			ActiveCommunityMissions.AddRange(missions);

			if (ActiveCommunityMissions.Count == 0)
				await AssignCommunityMissionsAsync();
		}

		public async Task<int> AssignDailyMissionsAsync(SeasonPlayer player)
		{
			var today = DateTime.UtcNow.Date;
			var hasOldMissions = player.PersonalMissions.Any(m => m.CreatedAt.Date < today);

			if (hasOldMissions)
			{
				await _database.DeleteOldMissionsAsync(player.SteamId, today);
				CheckStreak(player);
				player.PersonalMissions.Clear();
			}

			if (player.RerollResetDate < today)
			{
				player.Rerolls = Config.CurrentValue.Mission.DailyRerollCount +
					(player.HasBattlePass ? Config.CurrentValue.BattlePass.MissionRerollCount : 0) +
					(player.IsVip ? Config.CurrentValue.Vip.ExtraRerolls : 0);
				player.RerollResetDate = today;
			}

			var config = Config.CurrentValue;
			var totalCount = config.Mission.DailyMissionCount +
				(player.HasBattlePass ? config.BattlePass.DailyMissionCount : 0);

			if (player.PersonalMissions.Count >= totalCount)
				return 0;

			var needed = totalCount - player.PersonalMissions.Count;
			var available = _loader.GetPersonalMissions(player.HasBattlePass)
				.Where(m => !player.PersonalMissions.Any(pm =>
					pm.Event == m.Event && pm.Target == m.Target && pm.Name == m.Name))
				.ToList();

			var random = new Random();
			var now = DateTime.UtcNow;
			var assigned = 0;

			for (var i = 0; i < needed && available.Count > 0; i++)
			{
				var index = random.Next(available.Count);
				var def = available[index];
				available.RemoveAt(index);

				var mission = def.CreatePersonalMission(player.SteamId, now);
				var id = await _database.InsertMissionAsync(mission);

				if (id > 0)
				{
					mission.Id = id;
					player.PersonalMissions.Add(mission);
					assigned++;
				}
			}

			return assigned;
		}

		public async Task AssignCommunityMissionsAsync()
		{
			var config = Config.CurrentValue;
			await _database.DeleteOldCommunityMissionsAsync(DateTime.UtcNow.AddDays(-7));
			ActiveCommunityMissions.Clear();

			var communityDefs = _loader.GetCommunityMissions().ToList();
			var random = new Random();
			var now = DateTime.UtcNow;

			for (var i = 0; i < config.Mission.WeeklyMissionCount && communityDefs.Count > 0; i++)
			{
				var index = random.Next(communityDefs.Count);
				var def = communityDefs[index];
				communityDefs.RemoveAt(index);

				var mission = def.CreateCommunityMission(now);
				var id = await _database.InsertCommunityMissionAsync(mission);

				if (id > 0)
				{
					mission.Id = id;
					ActiveCommunityMissions.Add(mission);
				}
			}
		}

		public void ProcessEvent(SeasonPlayer player, string eventType, string target, Dictionary<string, object?>? props)
		{
			foreach (var mission in player.PersonalMissions.Where(m =>
				m.Matches(eventType, target, CurrentMap, props)))
			{
				mission.Progress++;

				if (mission.Progress >= mission.AmountToComplete)
					CompletePersonalMission(player, mission);
			}

			foreach (var mission in ActiveCommunityMissions.Where(m =>
				m.Matches(eventType, target, CurrentMap, props)))
			{
				mission.Progress++;

				if (mission.Progress >= mission.AmountToComplete)
					CompleteCommunityMission(mission);
			}
		}

		private void CompletePersonalMission(SeasonPlayer player, DbMission mission)
		{
			if (mission.Completed)
				return;

			mission.Completed = true;
			mission.CompletedAt = DateTime.UtcNow;
			Task.Run(async () => await _database.UpdateMissionAsync(mission));
			OnPersonalMissionCompleted?.Invoke(player, mission);
		}

		private void CompleteCommunityMission(DbCommunityMission mission)
		{
			if (mission.Completed)
				return;

			mission.Completed = true;
			mission.CompletedAt = DateTime.UtcNow;
			Task.Run(async () => await _database.UpdateCommunityMissionAsync(mission));
			OnCommunityMissionCompleted?.Invoke(mission);
		}

		public async Task SaveAllMissionsAsync(IEnumerable<SeasonPlayer> players)
		{
			foreach (var player in players.Where(p => p.IsLoaded))
			{
				foreach (var m in player.PersonalMissions.Where(m => m.Id > 0))
					await _database.UpdateMissionAsync(m);
			}

			foreach (var m in ActiveCommunityMissions.Where(m => m.Id > 0))
				await _database.UpdateCommunityMissionAsync(m);
		}

		public async Task AbandonMission(SeasonPlayer player, DbMission mission, PlayerManager playerManager)
		{
			if (mission.Completed || mission.AmountToComplete <= 0)
				return;

			var partialXp = (int)(mission.RewardExperience * ((float)mission.Progress / mission.AmountToComplete));

			if (partialXp > 0)
				playerManager.AddExperience(player, partialXp);

			player.PersonalMissions.Remove(mission);
			await _database.DeleteOldMissionsAsync(player.SteamId, DateTime.UtcNow.AddSeconds(1));
			player.LastFrustrationCheck = DateTime.UtcNow;
		}

		private static void CheckStreak(SeasonPlayer player)
		{
			var config = Config.CurrentValue;

			if (!config.Streak.Enabled)
				return;

			var completedYesterday = player.PersonalMissions
				.Count(m => m.Completed && m.CreatedAt.Date == DateTime.UtcNow.Date.AddDays(-1));

			if (completedYesterday >= config.Streak.RequiredDailyComplete)
				player.Streak++;
			else
				player.Streak = 0;
		}

		public void ProcessPlayTime(IEnumerable<SeasonPlayer> players)
		{
			foreach (var player in players.Where(p => p.IsValid && p.IsLoaded))
			{
				foreach (var mission in player.PersonalMissions.Where(m =>
					!m.Completed && m.Event == "PlayTime"))
				{
					mission.Progress++;

					if (mission.Progress >= mission.AmountToComplete)
						CompletePersonalMission(player, mission);
				}
			}

			foreach (var mission in ActiveCommunityMissions.Where(m =>
				!m.Completed && m.Event == "PlayTime"))
			{
				mission.Progress++;

				if (mission.Progress >= mission.AmountToComplete)
					CompleteCommunityMission(mission);
			}
		}

		public async Task<DbMission?> RerollMissionAsync(SeasonPlayer player, DbMission mission)
		{
			if (player.Rerolls <= 0 || mission.Completed)
				return null;

			player.Rerolls--;
			player.PersonalMissions.Remove(mission);

			if (mission.Id > 0)
				await _database.DeleteOldMissionsAsync(player.SteamId, DateTime.UtcNow.AddSeconds(1));

			var available = _loader.GetPersonalMissions(player.HasBattlePass)
				.Where(m => !player.PersonalMissions.Any(pm =>
					pm.Event == m.Event && pm.Target == m.Target && pm.Name == m.Name))
				.ToList();

			if (available.Count == 0)
				return null;

			var random = new Random();
			var def = available[random.Next(available.Count)];
			var newMission = def.CreatePersonalMission(player.SteamId, DateTime.UtcNow);
			var id = await _database.InsertMissionAsync(newMission);

			if (id > 0)
			{
				newMission.Id = id;
				player.PersonalMissions.Add(newMission);
				return newMission;
			}

			return null;
		}

		public bool ShouldResetCommunityMissions()
		{
			if (ActiveCommunityMissions.Count == 0)
				return true;

			var oldest = ActiveCommunityMissions.Min(m => m.CreatedAt);
			return (DateTime.UtcNow - oldest).TotalDays >= 7;
		}
	}
}

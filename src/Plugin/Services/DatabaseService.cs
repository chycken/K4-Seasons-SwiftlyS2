using System.Data;
using Dapper;
using Dommel;
using K4Seasons.Database.Migrations;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using MySqlConnector;

namespace K4Seasons;

public sealed partial class Plugin
{
	public sealed class DatabaseService(string connectionName)
	{
		private readonly string _connectionName = connectionName;
		public bool IsEnabled { get; private set; }

		public async Task InitializeAsync()
		{
			try
			{
				using var connection = Core.Database.GetConnection(_connectionName);
				MigrationRunner.RunMigrations(connection);
				IsEnabled = true;
				Core.Logger.LogInformation("Database initialized successfully.");
			}
			catch (Exception ex)
			{
				Core.Logger.LogError(ex, "Failed to initialize database.");
				IsEnabled = false;
			}
		}

		/* ==================== Players ==================== */

		public async Task<DbPlayer?> GetPlayerAsync(ulong steamId)
		{
			if (!IsEnabled)
				return null;

			try
			{
				using var conn = Core.Database.GetConnection(_connectionName);
				conn.Open();
				return await conn.GetAsync<DbPlayer>((long)steamId);
			}
			catch (Exception ex)
			{
				Core.Logger.LogError(ex, "Failed to load player {SteamId}", steamId);
				return null;
			}
		}

		public async Task<Dictionary<ulong, DateTime>> GetBattlePassDataAsync(IEnumerable<ulong> steamIds)
		{
			var result = new Dictionary<ulong, DateTime>();

			if (!IsEnabled || !steamIds.Any())
				return result;

			try
			{
				using var conn = Core.Database.GetConnection(_connectionName);
				conn.Open();

				var longIds = steamIds.Select(id => (long)id).ToList();
				var data = await conn.QueryAsync<(long steam_id, DateTime battle_pass_purchased)>(
					"SELECT steam_id, battle_pass_purchased FROM k4se_players WHERE steam_id IN @SteamIds AND battle_pass_purchased IS NOT NULL",
					new { SteamIds = longIds });

				foreach (var (steamId, purchaseDate) in data)
					result[(ulong)steamId] = purchaseDate;
			}
			catch (Exception ex)
			{
				Core.Logger.LogError(ex, "Failed to load battle pass data for multiple players");
			}

			return result;
		}

		public async Task UpsertPlayerAsync(SeasonPlayer player)
		{
			if (!IsEnabled)
				return;

			try
			{
				using var conn = Core.Database.GetConnection(_connectionName);
				conn.Open();

				var sql = IsMySql(conn)
					? @"INSERT INTO k4se_players (steam_id, username, experience, battle_pass_purchased, streak, prestige, rerolls, reroll_reset_date, active_time, last_seen)
						VALUES (@SteamId, @UserName, @Experience, @BattlePassPurchased, @Streak, @Prestige, @Rerolls, @RerollResetDate, @ActiveTime, @LastSeen)
						ON DUPLICATE KEY UPDATE
							username = @UserName, experience = @Experience, battle_pass_purchased = @BattlePassPurchased,
							streak = @Streak, prestige = @Prestige, rerolls = @Rerolls, reroll_reset_date = @RerollResetDate,
							active_time = @ActiveTime, last_seen = @LastSeen"
					: @"INSERT INTO k4se_players (steam_id, username, experience, battle_pass_purchased, streak, prestige, rerolls, reroll_reset_date, active_time, last_seen)
						VALUES (@SteamId, @UserName, @Experience, @BattlePassPurchased, @Streak, @Prestige, @Rerolls, @RerollResetDate, @ActiveTime, @LastSeen)
						ON CONFLICT (steam_id) DO UPDATE SET
							username = @UserName, experience = @Experience, battle_pass_purchased = @BattlePassPurchased,
							streak = @Streak, prestige = @Prestige, rerolls = @Rerolls, reroll_reset_date = @RerollResetDate,
							active_time = @ActiveTime, last_seen = @LastSeen";

				var dbPlayer = player.ToDbPlayer();
				await conn.ExecuteAsync(sql, dbPlayer);
			}
			catch (Exception ex)
			{
				Core.Logger.LogError(ex, "Failed to save player {SteamId}", player.SteamId);
			}
		}

		/* ==================== Claimed Rewards ==================== */

		public async Task<List<int>> GetClaimedLevelsAsync(ulong steamId)
		{
			if (!IsEnabled)
				return [];

			try
			{
				using var conn = Core.Database.GetConnection(_connectionName);
				conn.Open();
				var results = await conn.QueryAsync<int>(
					"SELECT level FROM k4se_claimed_rewards WHERE steam_id = @SteamId",
					new { SteamId = (long)steamId });
				return results.ToList();
			}
			catch (Exception ex)
			{
				Core.Logger.LogError(ex, "Failed to load claimed levels for {SteamId}", steamId);
				return [];
			}
		}

		public async Task InsertClaimedLevelAsync(ulong steamId, int level)
		{
			if (!IsEnabled)
				return;

			try
			{
				using var conn = Core.Database.GetConnection(_connectionName);
				conn.Open();

				var sql = IsMySql(conn)
					? "INSERT IGNORE INTO k4se_claimed_rewards (steam_id, level) VALUES (@SteamId, @Level)"
					: "INSERT OR IGNORE INTO k4se_claimed_rewards (steam_id, level) VALUES (@SteamId, @Level)";

				await conn.ExecuteAsync(sql, new { SteamId = (long)steamId, Level = level });
			}
			catch (Exception ex)
			{
				Core.Logger.LogError(ex, "Failed to insert claimed level {Level} for {SteamId}", level, steamId);
			}
		}

		public async Task DeleteClaimedLevelsAsync(ulong steamId)
		{
			if (!IsEnabled)
				return;

			try
			{
				using var conn = Core.Database.GetConnection(_connectionName);
				conn.Open();
				await conn.ExecuteAsync(
					"DELETE FROM k4se_claimed_rewards WHERE steam_id = @SteamId",
					new { SteamId = (long)steamId });
			}
			catch (Exception ex)
			{
				Core.Logger.LogError(ex, "Failed to delete claimed levels for {SteamId}", steamId);
			}
		}

		/* ==================== Personal Missions ==================== */

		public async Task<List<DbMission>> GetPlayerMissionsAsync(ulong steamId)
		{
			if (!IsEnabled)
				return [];

			try
			{
				using var conn = Core.Database.GetConnection(_connectionName);
				conn.Open();
				var results = await conn.SelectAsync<DbMission>(m => m.SteamId == (long)steamId && !m.Completed);
				return results.ToList();
			}
			catch (Exception ex)
			{
				Core.Logger.LogError(ex, "Failed to load missions for {SteamId}", steamId);
				return [];
			}
		}

		public async Task<int> InsertMissionAsync(DbMission mission)
		{
			if (!IsEnabled)
				return -1;

			try
			{
				using var conn = Core.Database.GetConnection(_connectionName);
				conn.Open();
				var id = await conn.InsertAsync(mission);
				return Convert.ToInt32(id);
			}
			catch (Exception ex)
			{
				Core.Logger.LogError(ex, "Failed to insert mission for {SteamId}", mission.SteamId);
				return -1;
			}
		}

		public async Task UpdateMissionAsync(DbMission mission)
		{
			if (!IsEnabled || mission.Id <= 0)
				return;

			try
			{
				using var conn = Core.Database.GetConnection(_connectionName);
				conn.Open();
				await conn.UpdateAsync(mission);
			}
			catch (Exception ex)
			{
				Core.Logger.LogError(ex, "Failed to update mission {Id}", mission.Id);
			}
		}

		public async Task DeleteOldMissionsAsync(ulong steamId, DateTime before)
		{
			if (!IsEnabled)
				return;

			try
			{
				using var conn = Core.Database.GetConnection(_connectionName);
				conn.Open();
				await conn.DeleteMultipleAsync<DbMission>(m => m.SteamId == (long)steamId && m.CreatedAt < before);
			}
			catch (Exception ex)
			{
				Core.Logger.LogError(ex, "Failed to delete old missions");
			}
		}

		/* ==================== Community Missions ==================== */

		public async Task<List<DbCommunityMission>> GetCommunityMissionsAsync()
		{
			if (!IsEnabled)
				return [];

			try
			{
				using var conn = Core.Database.GetConnection(_connectionName);
				conn.Open();
				var results = await conn.SelectAsync<DbCommunityMission>(m => !m.Completed);
				return results.ToList();
			}
			catch (Exception ex)
			{
				Core.Logger.LogError(ex, "Failed to load community missions");
				return [];
			}
		}

		public async Task<int> InsertCommunityMissionAsync(DbCommunityMission mission)
		{
			if (!IsEnabled)
				return -1;

			try
			{
				using var conn = Core.Database.GetConnection(_connectionName);
				conn.Open();
				var id = await conn.InsertAsync(mission);
				return Convert.ToInt32(id);
			}
			catch (Exception ex)
			{
				Core.Logger.LogError(ex, "Failed to insert community mission");
				return -1;
			}
		}

		public async Task UpdateCommunityMissionAsync(DbCommunityMission mission)
		{
			if (!IsEnabled || mission.Id <= 0)
				return;

			try
			{
				using var conn = Core.Database.GetConnection(_connectionName);
				conn.Open();
				await conn.UpdateAsync(mission);
			}
			catch (Exception ex)
			{
				Core.Logger.LogError(ex, "Failed to update community mission {Id}", mission.Id);
			}
		}

		public async Task DeleteOldCommunityMissionsAsync(DateTime before)
		{
			if (!IsEnabled)
				return;

			try
			{
				using var conn = Core.Database.GetConnection(_connectionName);
				conn.Open();
				await conn.DeleteMultipleAsync<DbCommunityMission>(m => m.CreatedAt < before);
			}
			catch (Exception ex)
			{
				Core.Logger.LogError(ex, "Failed to delete old community missions");
			}
		}

		/* ==================== Seasons ==================== */

		public async Task<DbSeason?> GetActiveSeasonAsync()
		{
			if (!IsEnabled)
				return null;

			try
			{
				using var conn = Core.Database.GetConnection(_connectionName);
				conn.Open();
				var results = await conn.SelectAsync<DbSeason>(s => s.IsActive);
				return results.FirstOrDefault();
			}
			catch (Exception ex)
			{
				Core.Logger.LogError(ex, "Failed to load active season");
				return null;
			}
		}

		public async Task<int> CreateSeasonAsync(DbSeason season)
		{
			if (!IsEnabled)
				return -1;

			try
			{
				using var conn = Core.Database.GetConnection(_connectionName);
				conn.Open();
				var id = await conn.InsertAsync(season);
				return Convert.ToInt32(id);
			}
			catch (Exception ex)
			{
				Core.Logger.LogError(ex, "Failed to create season");
				return -1;
			}
		}

		public async Task UpdateSeasonAsync(DbSeason season)
		{
			if (!IsEnabled)
				return;

			try
			{
				using var conn = Core.Database.GetConnection(_connectionName);
				conn.Open();
				await conn.UpdateAsync(season);
			}
			catch (Exception ex)
			{
				Core.Logger.LogError(ex, "Failed to update season");
			}
		}

		public async Task ResetSeasonDataAsync()
		{
			if (!IsEnabled)
				return;

			try
			{
				using var conn = Core.Database.GetConnection(_connectionName);
				conn.Open();
				await conn.ExecuteAsync("DELETE FROM k4se_personal_missions");
				await conn.ExecuteAsync("DELETE FROM k4se_community_missions");
				await conn.ExecuteAsync("DELETE FROM k4se_claimed_rewards");
				await conn.ExecuteAsync("DELETE FROM k4se_players");
			}
			catch (Exception ex)
			{
				Core.Logger.LogError(ex, "Failed to reset season data");
			}
		}

		/* ==================== Maintenance ==================== */

		public async Task PurgeOldDataAsync(int days)
		{
			if (!IsEnabled || days <= 0)
				return;

			try
			{
				var cutoff = DateTime.UtcNow.AddDays(-days);
				using var conn = Core.Database.GetConnection(_connectionName);
				conn.Open();

				await conn.ExecuteAsync(
					"DELETE FROM k4se_personal_missions WHERE steam_id IN (SELECT steam_id FROM k4se_players WHERE last_seen < @Cutoff)",
					new { Cutoff = cutoff });

				await conn.ExecuteAsync(
					"DELETE FROM k4se_claimed_rewards WHERE steam_id IN (SELECT steam_id FROM k4se_players WHERE last_seen < @Cutoff)",
					new { Cutoff = cutoff });

				await conn.DeleteMultipleAsync<DbPlayer>(p => p.LastSeen < cutoff);
			}
			catch (Exception ex)
			{
				Core.Logger.LogError(ex, "Failed to purge old data");
			}
		}

		public async Task<long> GetAverageBattlePassXPAsync()
		{
			if (!IsEnabled)
				return 0;

			try
			{
				using var conn = Core.Database.GetConnection(_connectionName);
				conn.Open();
				var result = await conn.ExecuteScalarAsync<long?>(
					"SELECT AVG(experience) FROM k4se_players WHERE battle_pass_purchased IS NOT NULL");
				return result ?? 0;
			}
			catch (Exception ex)
			{
				Core.Logger.LogError(ex, "Failed to get average BP XP");
				return 0;
			}
		}

		public async Task<List<ToplistEntry>> GetTopPlayersAsync(int count)
		{
			if (!IsEnabled)
				return [];

			try
			{
				using var conn = Core.Database.GetConnection(_connectionName);
				conn.Open();
				var results = await conn.QueryAsync<DbPlayer>(
					"SELECT * FROM k4se_players ORDER BY experience DESC LIMIT @Count",
					new { Count = count });

				return results.Select((p, i) => new ToplistEntry
				{
					Rank = i + 1,
					SteamId = (ulong)p.SteamId,
					UserName = p.UserName,
					Experience = p.Experience
				}).ToList();
			}
			catch (Exception ex)
			{
				Core.Logger.LogError(ex, "Failed to get toplist");
				return [];
			}
		}

		public async Task<(int rank, int total)> GetPlayerRankAsync(ulong steamId)
		{
			if (!IsEnabled)
				return (0, 0);

			try
			{
				using var conn = Core.Database.GetConnection(_connectionName);
				conn.Open();

				var total = await conn.QuerySingleAsync<int>("SELECT COUNT(*) FROM k4se_players WHERE experience > 0");
				var rank = await conn.QuerySingleAsync<int>(
					"SELECT COUNT(*) + 1 FROM k4se_players WHERE experience > (SELECT experience FROM k4se_players WHERE steam_id = @SteamId)",
					new { SteamId = (long)steamId });

				return (rank, total);
			}
			catch (Exception ex)
			{
				Core.Logger.LogError(ex, "Failed to get player rank for {SteamId}", steamId);
				return (0, 0);
			}
		}

		/* ==================== Helpers ==================== */

		private static bool IsMySql(IDbConnection conn) => conn is MySqlConnection;
	}
}

using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace K4Seasons;

public sealed partial class Plugin
{
	public sealed class ToplistService(DatabaseService database)
	{
		private readonly DatabaseService _database = database;
		private readonly ConcurrentDictionary<int, ToplistEntry> _toplist = new();
		private DateTime _lastUpdate = DateTime.MinValue;

		public IReadOnlyDictionary<int, ToplistEntry> Toplist => _toplist;

		public async Task UpdateAsync()
		{
			if ((DateTime.UtcNow - _lastUpdate).TotalMinutes < 5)
				return;

			try
			{
				var maxRank = Config.CurrentValue.Toplist.PositionMultipliers.Count;
				var entries = await _database.GetTopPlayersAsync(maxRank);

				_toplist.Clear();

				foreach (var entry in entries)
					_toplist[entry.Rank] = entry;

				_lastUpdate = DateTime.UtcNow;
			}
			catch (Exception ex)
			{
				Core.Logger.LogError(ex, "Failed to update toplist");
			}
		}
	}
}

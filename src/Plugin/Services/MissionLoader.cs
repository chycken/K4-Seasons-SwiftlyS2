using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace K4Seasons;

public sealed partial class Plugin
{
	public sealed class MissionLoader
	{
		private readonly List<MissionDefinition> _missions = [];

		public void LoadFromFile(string moduleDirectory)
		{
			var filePath = Path.Combine(moduleDirectory, "missions.json");
			if (!File.Exists(filePath))
			{
				var resourcePath = Path.Combine(moduleDirectory, "resources", "missions.json");
				if (File.Exists(resourcePath))
					File.Copy(resourcePath, filePath);
				else
				{
					Core.Logger.LogError("missions.json not found.");
					return;
				}
			}

			try
			{
				var json = File.ReadAllText(filePath);
				var loaded = JsonSerializer.Deserialize<List<MissionDefinition>>(json, new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true,
					ReadCommentHandling = JsonCommentHandling.Skip,
					Converters = { new JsonStringEnumConverter() }
				});

				if (loaded is { Count: > 0 })
				{
					_missions.Clear();
					_missions.AddRange(loaded);
					Core.Logger.LogInformation("Loaded {Count} mission definitions.", _missions.Count);
				}
			}
			catch (Exception ex)
			{
				Core.Logger.LogError(ex, "Failed to load missions.json");
			}
		}

		public IReadOnlyList<MissionDefinition> GetAllMissions() => _missions;

		public IEnumerable<MissionDefinition> GetPersonalMissions(bool hasBattlePass) =>
			_missions.Where(m => m.Type == MissionType.Personal && (!m.BattlePassOnly || hasBattlePass));

		public IEnumerable<MissionDefinition> GetCommunityMissions() =>
			_missions.Where(m => m.Type == MissionType.Community);
	}
}

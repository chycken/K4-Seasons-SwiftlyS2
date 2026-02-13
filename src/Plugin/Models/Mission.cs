using System.Text.Json;

namespace K4Seasons;

public sealed class MissionDefinition
{
	public string Event { get; set; } = string.Empty;
	public Dictionary<string, JsonElement>? EventProperties { get; set; }
	public string Target { get; set; } = string.Empty;
	public MissionType Type { get; set; } = MissionType.Personal;
	public string Name { get; set; } = string.Empty;
	public int AmountToComplete { get; set; }
	public int RewardExperience { get; set; }
	public bool BattlePassOnly { get; set; }
	public string? Map { get; set; }

	public DbMission CreatePersonalMission(ulong steamId, DateTime createdAt) => new()
	{
		SteamId = (long)steamId,
		Event = Event,
		Target = Target,
		Name = Name,
		AmountToComplete = AmountToComplete,
		RewardExperience = RewardExperience,
		BattlePassOnly = BattlePassOnly,
		EventProperties = EventProperties,
		Map = Map,
		CreatedAt = createdAt
	};

	public DbCommunityMission CreateCommunityMission(DateTime createdAt) => new()
	{
		Event = Event,
		Target = Target,
		Name = Name,
		AmountToComplete = AmountToComplete,
		RewardExperience = RewardExperience,
		EventProperties = EventProperties,
		Map = Map,
		CreatedAt = createdAt
	};
}

namespace K4Seasons;

public sealed class SeasonConfig
{
	public string SeasonName { get; set; } = string.Empty;
	public int DurationDays { get; set; }
	public Dictionary<int, SeasonReward> Rewards { get; set; } = new();
}

public sealed class SeasonReward
{
	public string Name { get; set; } = string.Empty;
	public List<string> Commands { get; set; } = [];
	public List<string> Permissions { get; set; } = [];
	public bool BattlePassOnly { get; set; }
}

namespace K4Seasons;

public sealed class ToplistEntry
{
	public int Rank { get; set; }
	public ulong SteamId { get; set; }
	public string UserName { get; set; } = string.Empty;
	public long Experience { get; set; }
}

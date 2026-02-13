using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace K4Seasons;

[Table("k4se_players")]
public sealed class DbPlayer
{
	[Key]
	[Column("steam_id")]
	public long SteamId { get; set; }

	[Column("username")]
	public string UserName { get; set; } = string.Empty;

	[Column("experience")]
	public long Experience { get; set; }

	[Column("battle_pass_purchased")]
	public DateTime? BattlePassPurchased { get; set; }

	[Column("streak")]
	public int Streak { get; set; }

	[Column("prestige")]
	public int Prestige { get; set; }

	[Column("rerolls")]
	public int Rerolls { get; set; }

	[Column("reroll_reset_date")]
	public DateTime RerollResetDate { get; set; }

	[Column("active_time")]
	public int ActiveTime { get; set; }

	[Column("last_seen")]
	public DateTime LastSeen { get; set; }
}

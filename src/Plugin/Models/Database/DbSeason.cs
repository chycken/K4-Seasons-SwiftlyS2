using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace K4Seasons;

[Table("k4se_seasons")]
public sealed class DbSeason
{
	[Key]
	[Column("id")]
	public int Id { get; set; }

	[Column("season_name")]
	public string SeasonName { get; set; } = string.Empty;

	[Column("start_date")]
	public DateTime StartDate { get; set; }

	[Column("end_date")]
	public DateTime EndDate { get; set; }

	[Column("is_active")]
	public bool IsActive { get; set; }
}

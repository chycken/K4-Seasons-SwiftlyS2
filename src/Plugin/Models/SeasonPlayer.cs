using SwiftlyS2.Shared.Players;

namespace K4Seasons;

public sealed class SeasonPlayer
{
	public required ulong SteamId { get; init; }
	public required IPlayer Player { get; init; }

	public string UserName { get; set; } = "Unknown";
	public long Experience { get; set; }
	public DateTime? BattlePassPurchased { get; set; }
	public int Streak { get; set; }
	public int Prestige { get; set; }
	public int Rerolls { get; set; }
	public DateTime RerollResetDate { get; set; } = DateTime.MinValue;
	public int ActiveTime { get; set; }
	public List<int> ClaimedBattlePassLevels { get; set; } = [];
	public List<DbMission> PersonalMissions { get; set; } = [];
	public long CatchupTargetXP { get; set; }
	public DateTime LastFrustrationCheck { get; set; } = DateTime.MinValue;
	public DateTime LastPrestigeReminder { get; set; } = DateTime.MinValue;
	public int? FrustratedMissionId { get; set; }
	public bool IsVip { get; set; }

	public bool IsLoaded { get; set; }
	public bool HasBattlePass => BattlePassPurchased.HasValue;
	public bool IsValid => Player.IsValid && !Player.IsFakeClient;

	public int MaxLevel(PluginConfig config) =>
		HasBattlePass ? config.BattlePass.LevelCap : config.Level.LevelCap;

	public int GetLevel(PluginConfig config, long? experience = null)
	{
		var xp = experience ?? Experience;

		if (xp <= 0)
			return 0;

		if (config.Level.ExperienceDynamicMultiplier <= 1.0f)
			return (int)(xp / config.Level.ExperiencePerLevel);

		var level = 0;
		var totalRequired = 0L;
		var maxLvl = MaxLevel(config);

		while (level < maxLvl)
		{
			var required = (long)(config.Level.ExperiencePerLevel *
				Math.Pow(config.Level.ExperienceDynamicMultiplier, level));
			totalRequired += required;

			if (xp < totalRequired)
				break;

			level++;
		}

		return Math.Min(level, maxLvl);
	}

	public static long GetRequiredExperience(PluginConfig config, int targetLevel)
	{
		if (config.Level.ExperienceDynamicMultiplier <= 1.0f)
			return (long)targetLevel * config.Level.ExperiencePerLevel;

		var total = 0L;

		for (var i = 0; i < targetLevel; i++)
		{
			total += (long)(config.Level.ExperiencePerLevel *
				Math.Pow(config.Level.ExperienceDynamicMultiplier, i));
		}

		return total;
	}

	public float GetCurrentMultiplier(PluginConfig config, IReadOnlyDictionary<int, ToplistEntry>? toplist)
	{
		var multiplier = 1.0f;

		if (config.Streak.Enabled && Streak > 0)
		{
			foreach (var (days, bonus) in config.Streak.DaySettings.OrderByDescending(x => x.Key))
			{
				if (Streak >= days)
				{
					multiplier *= bonus;
					break;
				}
			}
		}

		if (config.WeeklyBonus.Enabled)
		{
			var dayName = DateTime.Now.DayOfWeek.ToString();

			if (config.WeeklyBonus.DayMultipliers.TryGetValue(dayName, out var dayMultiplier))
				multiplier *= dayMultiplier;
		}

		if (config.PrimeTime.Enabled && IsPrimeTime(config))
			multiplier *= config.PrimeTime.Multiplier;

		if (HasBattlePass)
			multiplier *= config.BattlePass.Multiplier;

		if (Prestige > 0)
			multiplier *= (float)Math.Pow(config.Prestige.MultiplierPerLevel, Prestige);

		if (HasBattlePass && config.CatchUp.Enabled)
			multiplier *= GetCatchUpMultiplier(config);

		if (config.Toplist.Enabled && toplist != null)
		{
			var entry = toplist.Values.FirstOrDefault(t => t.SteamId == SteamId);

			if (entry != null && config.Toplist.PositionMultipliers.TryGetValue(entry.Rank, out var bonus))
				multiplier *= bonus;
		}

		if (IsVip && config.Vip.Multiplier > 1.0f)
			multiplier *= config.Vip.Multiplier;

		return multiplier;
	}

	public DbPlayer ToDbPlayer() => new()
	{
		SteamId = (long)SteamId,
		UserName = UserName,
		Experience = Experience,
		BattlePassPurchased = BattlePassPurchased,
		Streak = Streak,
		Prestige = Prestige,
		Rerolls = Rerolls,
		RerollResetDate = RerollResetDate,
		ActiveTime = ActiveTime,
		LastSeen = DateTime.UtcNow
	};

	private float GetCatchUpMultiplier(PluginConfig config)
	{
		if (!HasBattlePass || CatchupTargetXP <= 0 || Experience >= CatchupTargetXP)
			return 1.0f;

		var weeksSincePurchase = (DateTime.UtcNow - BattlePassPurchased!.Value).TotalDays / 7.0;
		return (float)Math.Pow(config.CatchUp.MultiplierPerWeek, Math.Max(0, weeksSincePurchase));
	}

	private static bool IsPrimeTime(PluginConfig config)
	{
		if (!TimeSpan.TryParse(config.PrimeTime.Start, out var start) ||
			!TimeSpan.TryParse(config.PrimeTime.End, out var end))
			return false;

		var now = DateTime.Now.TimeOfDay;

		return start <= end
			? now >= start && now <= end
			: now >= start || now <= end;
	}
}

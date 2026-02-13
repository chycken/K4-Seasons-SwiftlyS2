namespace K4Seasons;

/// <summary>
/// Main plugin configuration (config.json)
/// </summary>
public sealed class PluginConfig
{
	public DatabaseSettings Database { get; set; } = new();
	public GeneralSettings General { get; set; } = new();
	public CommandSettings Commands { get; set; } = new();
	public ExperienceSettings Experience { get; set; } = new();
	public LevelSettings Level { get; set; } = new();
	public MissionSettings Mission { get; set; } = new();
	public BattlePassSettings BattlePass { get; set; } = new();
	public PrestigeSettings Prestige { get; set; } = new();
	public VipSettings Vip { get; set; } = new();
	public StreakSettings Streak { get; set; } = new();
	public WeeklyBonusSettings WeeklyBonus { get; set; } = new();
	public PrimeTimeSettings PrimeTime { get; set; } = new();
	public CatchUpSettings CatchUp { get; set; } = new();
	public ToplistSettings Toplist { get; set; } = new();
	public AntiFrustrationSettings AntiFrustration { get; set; } = new();
}

/* ==================== Database ==================== */

public sealed class DatabaseSettings
{
	/// <summary>DB connection name (from SwiftlyS2's database.jsonc)</summary>
	public string Connection { get; set; } = "default";

	/// <summary>Days to keep inactive player records (0 = forever)</summary>
	public int PurgeDays { get; set; } = 90;
}

/* ==================== General ==================== */

public sealed class GeneralSettings
{
	/// <summary>Date format used in chat messages</summary>
	public string DateFormat { get; set; } = "yyyy/MM/dd";

	/// <summary>Save all player data at the end of each round</summary>
	public bool RoundEndSave { get; set; } = true;
}

/* ==================== Commands ==================== */

public sealed class CommandSettings
{
	/// <summary>Season menu command</summary>
	public CommandConfig Season { get; set; } = new()
	{
		Command = "seasons",
		Aliases = ["season"]
	};

	/// <summary>Prestige command</summary>
	public CommandConfig Prestige { get; set; } = new()
	{
		Command = "prestige",
		Aliases = []
	};

	/// <summary>Admin command to give Battle Pass</summary>
	public CommandConfig GiveBattlePass { get; set; } = new()
	{
		Command = "givebp",
		Aliases = [],
		Permission = "k4-seasons.admin"
	};

	/// <summary>Reroll mission command (usage: !reroll 1)</summary>
	public CommandConfig RerollMission { get; set; } = new()
	{
		Command = "reroll",
		Aliases = []
	};

	/// <summary>Abandon mission command (usage: !abandon 1)</summary>
	public CommandConfig AbandonMission { get; set; } = new()
	{
		Command = "abandon",
		Aliases = []
	};
}

/// <summary>
/// Single command configuration
/// </summary>
public sealed class CommandConfig
{
	public string Command { get; set; } = "";
	public List<string> Aliases { get; set; } = [];
	public string Permission { get; set; } = "";
}

/* ==================== Experience ==================== */

public sealed class ExperienceSettings
{
	/// <summary>Enable built-in XP reward events (kill, bomb, etc.)</summary>
	public bool EventsEnabled { get; set; } = true;

	/// <summary>Award XP during warmup</summary>
	public bool WarmupExperience { get; set; } = true;

	/// <summary>Show XP progress at round end</summary>
	public bool ShowProgressOnDeath { get; set; } = true;

	/// <summary>Show XP progress every N minutes (0 = disabled)</summary>
	public int ShowProgressOnMinutes { get; set; } = 0;

	/// <summary>Minimum connected players for XP rewards (prevents farming)</summary>
	public int MinPlayerCount { get; set; } = 4;

	/// <summary>XP reward for round MVP</summary>
	public int MvpReward { get; set; } = 75;

	/// <summary>XP reward for rescuing a hostage</summary>
	public int HostageRescueReward { get; set; } = 100;

	/// <summary>XP reward for defusing the bomb</summary>
	public int BombDefuseReward { get; set; } = 150;

	/// <summary>XP reward for planting the bomb</summary>
	public int BombPlantReward { get; set; } = 75;

	/// <summary>XP reward for getting a kill</summary>
	public int KillReward { get; set; } = 25;

	/// <summary>XP reward for winning a round</summary>
	public int RoundWinReward { get; set; } = 150;

	/// <summary>XP reward for winning the match</summary>
	public int GameWinReward { get; set; } = 500;
}

/* ==================== Level ==================== */

public sealed class LevelSettings
{
	/// <summary>Base XP required per level</summary>
	public int ExperiencePerLevel { get; set; } = 1500;

	/// <summary>XP scaling multiplier per level (1.0 = flat, >1.0 = progressive)</summary>
	public float ExperienceDynamicMultiplier { get; set; } = 1.0f;

	/// <summary>Maximum level for non-Battle Pass players</summary>
	public int LevelCap { get; set; } = 20;
}

/* ==================== Mission ==================== */

public sealed class MissionSettings
{
	/// <summary>Track mission progress during warmup</summary>
	public bool RecordWarmup { get; set; } = true;

	/// <summary>Minimum connected players for mission progress</summary>
	public int MinPlayerCount { get; set; } = 4;

	/// <summary>Number of daily personal missions assigned</summary>
	public int DailyMissionCount { get; set; } = 3;

	/// <summary>Number of weekly community missions</summary>
	public int WeeklyMissionCount { get; set; } = 2;

	/// <summary>Daily mission reroll count</summary>
	public int DailyRerollCount { get; set; } = 1;

	/// <summary>Log mission event details for debugging</summary>
	public bool EventDebugLogs { get; set; } = false;
}

/* ==================== Battle Pass ==================== */

public sealed class BattlePassSettings
{
	/// <summary>XP multiplier for Battle Pass holders</summary>
	public float Multiplier { get; set; } = 1.5f;

	/// <summary>Extra daily missions for Battle Pass holders</summary>
	public int DailyMissionCount { get; set; } = 2;

	/// <summary>Extra daily rerolls for Battle Pass holders</summary>
	public int MissionRerollCount { get; set; } = 2;

	/// <summary>Maximum level for Battle Pass holders</summary>
	public int LevelCap { get; set; } = 100;
}

/* ==================== Prestige ==================== */

public sealed class PrestigeSettings
{
	/// <summary>Enable prestige system</summary>
	public bool Enabled { get; set; } = false;

	/// <summary>Maximum prestige level</summary>
	public int LevelCap { get; set; } = 5;

	/// <summary>XP multiplier bonus per prestige level</summary>
	public float MultiplierPerLevel { get; set; } = 1.2f;

	/// <summary>Minutes between prestige reminder messages (0 = disabled)</summary>
	public int ReminderMinuteInterval { get; set; } = 10;
}

/* ==================== VIP ==================== */

public sealed class VipSettings
{
	/// <summary>Permission flags that grant VIP status (any one of these)</summary>
	public List<string> Flags { get; set; } = [];

	/// <summary>XP multiplier for VIP players</summary>
	public float Multiplier { get; set; } = 1.25f;

	/// <summary>Extra daily mission rerolls for VIP players</summary>
	public int ExtraRerolls { get; set; } = 1;
}

/* ==================== Streak ==================== */

public sealed class StreakSettings
{
	/// <summary>Enable daily streak XP multiplier</summary>
	public bool Enabled { get; set; } = true;

	/// <summary>Completed daily missions needed to maintain streak</summary>
	public int RequiredDailyComplete { get; set; } = 3;

	/// <summary>Streak day thresholds and their XP multipliers</summary>
	public Dictionary<int, float> DaySettings { get; set; } = new()
	{
		{ 3, 1.1f },
		{ 5, 1.25f },
		{ 7, 1.5f }
	};
}

/* ==================== Weekly Bonus ==================== */

public sealed class WeeklyBonusSettings
{
	/// <summary>Enable day-of-week XP multipliers</summary>
	public bool Enabled { get; set; } = false;

	/// <summary>Day name and its XP multiplier</summary>
	public Dictionary<string, float> DayMultipliers { get; set; } = new()
	{
		{ "Monday", 1.0f },
		{ "Tuesday", 1.0f },
		{ "Wednesday", 1.0f },
		{ "Thursday", 1.0f },
		{ "Friday", 1.0f },
		{ "Saturday", 1.2f },
		{ "Sunday", 1.2f }
	};
}

/* ==================== Prime Time ==================== */

public sealed class PrimeTimeSettings
{
	/// <summary>Enable time-based XP multiplier</summary>
	public bool Enabled { get; set; } = false;

	/// <summary>Prime time start (HH:mm format)</summary>
	public string Start { get; set; } = "20:00";

	/// <summary>Prime time end (HH:mm format)</summary>
	public string End { get; set; } = "08:00";

	/// <summary>XP multiplier during prime time</summary>
	public float Multiplier { get; set; } = 1.1f;
}

/* ==================== Catch Up ==================== */

public sealed class CatchUpSettings
{
	/// <summary>Enable catch-up XP bonus for Battle Pass holders behind the average</summary>
	public bool Enabled { get; set; } = false;

	/// <summary>XP multiplier bonus per week behind average</summary>
	public float MultiplierPerWeek { get; set; } = 1.2f;
}

/* ==================== Toplist ==================== */

public sealed class ToplistSettings
{
	/// <summary>Enable XP multipliers for top-ranked players</summary>
	public bool Enabled { get; set; } = true;

	/// <summary>Leaderboard position and its XP multiplier</summary>
	public Dictionary<int, float> PositionMultipliers { get; set; } = new()
	{
		{ 1, 1.10f },
		{ 2, 1.09f },
		{ 3, 1.08f },
		{ 4, 1.07f },
		{ 5, 1.06f },
		{ 6, 1.05f },
		{ 7, 1.04f },
		{ 8, 1.03f },
		{ 9, 1.02f },
		{ 10, 1.01f }
	};
}

/* ==================== Anti-Frustration ==================== */

public sealed class AntiFrustrationSettings
{
	/// <summary>Enable anti-frustration mission abandon hints</summary>
	public bool Enabled { get; set; } = false;

	/// <summary>Minimum active time in minutes before checking for frustration</summary>
	public int MinuteInterval { get; set; } = 60;

	/// <summary>Cooldown in minutes between frustration checks per player</summary>
	public int BetweenDelay { get; set; } = 15;
}

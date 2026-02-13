using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace K4Seasons;

[Table("k4se_personal_missions")]
public sealed class DbMission
{
	[Key]
	[Column("id")]
	public int Id { get; set; }

	[Column("steam_id")]
	public long SteamId { get; set; }

	[Column("event_name")]
	public string Event { get; set; } = string.Empty;

	[Column("target")]
	public string Target { get; set; } = string.Empty;

	[Column("mission_name")]
	public string Name { get; set; } = string.Empty;

	[Column("amount_to_complete")]
	public int AmountToComplete { get; set; }

	[Column("current_progress")]
	public int Progress { get; set; }

	[Column("reward_experience")]
	public int RewardExperience { get; set; }

	[Column("battle_pass_only")]
	public bool BattlePassOnly { get; set; }

	[Column("completed")]
	public bool Completed { get; set; }

	[Column("event_properties")]
	public string? EventPropertiesJson { get; set; }

	[Column("map")]
	public string? Map { get; set; }

	[Column("created_at")]
	public DateTime CreatedAt { get; set; }

	[Column("completed_at")]
	public DateTime? CompletedAt { get; set; }

	[NotMapped]
	private Dictionary<string, JsonElement>? _eventProperties;

	[NotMapped]
	public Dictionary<string, JsonElement>? EventProperties
	{
		get
		{
			if (_eventProperties == null && !string.IsNullOrEmpty(EventPropertiesJson))
			{
				try { _eventProperties = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(EventPropertiesJson); }
				catch { }
			}

			return _eventProperties;
		}
		set
		{
			_eventProperties = value;
			EventPropertiesJson = value is { Count: > 0 } ? JsonSerializer.Serialize(value) : null;
		}
	}

	public bool Matches(string eventType, string target, string? currentMap, Dictionary<string, object?>? eventProperties)
	{
		if (Completed)
			return false;

		if (!string.Equals(Event, eventType, StringComparison.OrdinalIgnoreCase))
			return false;

		if (!string.Equals(Target, target, StringComparison.OrdinalIgnoreCase))
			return false;

		if (Map != null && !string.Equals(Map, currentMap, StringComparison.OrdinalIgnoreCase))
			return false;

		if (EventProperties != null && eventProperties != null)
			return MatchesProperties(eventProperties);

		return true;
	}

	private bool MatchesProperties(Dictionary<string, object?> eventProperties)
	{
		if (EventProperties == null)
			return true;

		foreach (var (key, missionValue) in EventProperties)
		{
			if (!eventProperties.TryGetValue(key, out var eventValue) || eventValue == null)
				return false;

			if (!CompareValue(missionValue, eventValue))
				return false;
		}

		return true;
	}

	internal static bool CompareValue(JsonElement missionValue, object eventValue) => missionValue.ValueKind switch
	{
		JsonValueKind.True or JsonValueKind.False =>
			eventValue is bool b && missionValue.GetBoolean() == b,

		JsonValueKind.Number when missionValue.TryGetInt64(out var l) =>
			CompareNumeric(eventValue, l),

		JsonValueKind.Number when missionValue.TryGetDouble(out var d) =>
			CompareDouble(eventValue, d),

		JsonValueKind.String when missionValue.GetString() is { } s =>
			eventValue is string es && es.Contains(s, StringComparison.OrdinalIgnoreCase),

		_ => false
	};

	internal static bool CompareNumeric(object val, long target) => val switch
	{
		byte v => v >= target,
		sbyte v => v >= target,
		short v => v >= target,
		ushort v => v >= target,
		int v => v >= target,
		uint v => v >= target,
		long v => v >= target,
		ulong v => v >= (ulong)target,
		float v => v >= target,
		double v => v >= target,
		_ => false
	};

	internal static bool CompareDouble(object val, double target) => val switch
	{
		float v => v >= target,
		double v => v >= target,
		int v => v >= target,
		long v => v >= target,
		_ => false
	};
}

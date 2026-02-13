using FluentMigrator;

namespace K4Seasons.Database.Migrations;

[Migration(202602130001)]
public class Season_InitialSchema : Migration
{
	public override void Up()
	{
		if (!Schema.Table("k4se_players").Exists())
		{
			Create.Table("k4se_players")
				.WithColumn("steam_id").AsInt64().NotNullable().PrimaryKey()
				.WithColumn("username").AsString(128).NotNullable()
				.WithColumn("experience").AsInt64().NotNullable().WithDefaultValue(0)
				.WithColumn("battle_pass_purchased").AsDateTime().Nullable()
				.WithColumn("streak").AsInt32().NotNullable().WithDefaultValue(0)
				.WithColumn("prestige").AsInt32().NotNullable().WithDefaultValue(0)
				.WithColumn("rerolls").AsInt32().NotNullable().WithDefaultValue(0)
				.WithColumn("reroll_reset_date").AsDateTime().NotNullable()
				.WithColumn("active_time").AsInt32().NotNullable().WithDefaultValue(0)
				.WithColumn("last_seen").AsDateTime().NotNullable();

			Create.Index("idx_k4se_players_last_seen").OnTable("k4se_players").OnColumn("last_seen");
		}

		if (!Schema.Table("k4se_claimed_rewards").Exists())
		{
			Create.Table("k4se_claimed_rewards")
				.WithColumn("id").AsInt32().NotNullable().PrimaryKey().Identity()
				.WithColumn("steam_id").AsInt64().NotNullable()
				.WithColumn("level").AsInt32().NotNullable();

			Create.Index("idx_claimed_rewards_unique")
				.OnTable("k4se_claimed_rewards")
				.OnColumn("steam_id").Ascending()
				.OnColumn("level").Ascending()
				.WithOptions().Unique();
		}

		if (!Schema.Table("k4se_personal_missions").Exists())
		{
			Create.Table("k4se_personal_missions")
				.WithColumn("id").AsInt32().NotNullable().PrimaryKey().Identity()
				.WithColumn("steam_id").AsInt64().NotNullable()
				.WithColumn("event_name").AsString(64).NotNullable()
				.WithColumn("target").AsString(64).NotNullable()
				.WithColumn("mission_name").AsString(255).NotNullable()
				.WithColumn("amount_to_complete").AsInt32().NotNullable()
				.WithColumn("current_progress").AsInt32().NotNullable().WithDefaultValue(0)
				.WithColumn("reward_experience").AsInt32().NotNullable()
				.WithColumn("battle_pass_only").AsBoolean().NotNullable().WithDefaultValue(false)
				.WithColumn("completed").AsBoolean().NotNullable().WithDefaultValue(false)
				.WithColumn("event_properties").AsString(int.MaxValue).Nullable()
				.WithColumn("map").AsString(64).Nullable()
				.WithColumn("created_at").AsDateTime().NotNullable()
				.WithColumn("completed_at").AsDateTime().Nullable();

			Create.Index("idx_k4se_missions_steam_completed")
				.OnTable("k4se_personal_missions")
				.OnColumn("steam_id").Ascending()
				.OnColumn("completed").Ascending();

			Create.Index("idx_k4se_missions_created").OnTable("k4se_personal_missions").OnColumn("created_at");
		}

		if (!Schema.Table("k4se_community_missions").Exists())
		{
			Create.Table("k4se_community_missions")
				.WithColumn("id").AsInt32().NotNullable().PrimaryKey().Identity()
				.WithColumn("event_name").AsString(64).NotNullable()
				.WithColumn("target").AsString(64).NotNullable()
				.WithColumn("mission_name").AsString(255).NotNullable()
				.WithColumn("amount_to_complete").AsInt32().NotNullable()
				.WithColumn("current_progress").AsInt32().NotNullable().WithDefaultValue(0)
				.WithColumn("reward_experience").AsInt32().NotNullable()
				.WithColumn("completed").AsBoolean().NotNullable().WithDefaultValue(false)
				.WithColumn("event_properties").AsString(int.MaxValue).Nullable()
				.WithColumn("map").AsString(64).Nullable()
				.WithColumn("created_at").AsDateTime().NotNullable()
				.WithColumn("completed_at").AsDateTime().Nullable();
		}

		if (!Schema.Table("k4se_seasons").Exists())
		{
			Create.Table("k4se_seasons")
				.WithColumn("id").AsInt32().NotNullable().PrimaryKey().Identity()
				.WithColumn("season_name").AsString(128).NotNullable()
				.WithColumn("start_date").AsDateTime().NotNullable()
				.WithColumn("end_date").AsDateTime().NotNullable()
				.WithColumn("is_active").AsBoolean().NotNullable().WithDefaultValue(false);
		}
	}

	public override void Down()
	{
		if (Schema.Table("k4se_seasons").Exists())
			Delete.Table("k4se_seasons");

		if (Schema.Table("k4se_community_missions").Exists())
			Delete.Table("k4se_community_missions");

		if (Schema.Table("k4se_personal_missions").Exists())
			Delete.Table("k4se_personal_missions");

		if (Schema.Table("k4se_claimed_rewards").Exists())
			Delete.Table("k4se_claimed_rewards");

		if (Schema.Table("k4se_players").Exists())
			Delete.Table("k4se_players");
	}
}

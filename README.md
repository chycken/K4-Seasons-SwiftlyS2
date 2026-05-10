<h1><a>FIXED ISSUE: PLUGIN DIDNT RESPECT THE MINPLAYER SETTINGS</a></h1>

![GitHub tag (with filter)](https://img.shields.io/github/v/tag/K4ryuu/K4-Seasons-SwiftlyS2?style=for-the-badge&label=Version)
![GitHub Repo stars](https://img.shields.io/github/stars/K4ryuu/K4-Seasons-SwiftlyS2?style=for-the-badge)
![GitHub issues](https://img.shields.io/github/issues/K4ryuu/K4-Seasons-SwiftlyS2?style=for-the-badge)
![GitHub](https://img.shields.io/github/license/K4ryuu/K4-Seasons-SwiftlyS2?style=for-the-badge)
![GitHub all releases](https://img.shields.io/github/downloads/K4ryuu/K4-Seasons-SwiftlyS2/total?style=for-the-badge)
[![Discord](https://img.shields.io/badge/Discord-Join%20Server-5865F2?style=for-the-badge&logo=discord&logoColor=white)](https://dsc.gg/k4-fanbase)

<!-- PROJECT LOGO -->
<br />
<div align="center">
  <h1 align="center">KitsuneLab©</h1>
  <h3 align="center">K4-Seasons</h3>
  <a align="center">A comprehensive battle pass and season system for Counter-Strike 2 using SwiftlyS2 framework. Features XP leveling, daily missions, community goals, prestige, streaks, and deep multiplier stacking.</a>

  <p align="center">
    <br />
    <a href="https://github.com/K4ryuu/K4-Seasons-SwiftlyS2/releases/latest">Download</a>
    ·
    <a href="https://github.com/K4ryuu/K4-Seasons-SwiftlyS2/issues/new?assignees=K4ryuu&labels=bug&projects=&template=bug_report.md&title=%5BBUG%5D">Report Bug</a>
    ·
    <a href="https://github.com/K4ryuu/K4-Seasons-SwiftlyS2/issues/new?assignees=K4ryuu&labels=enhancement&projects=&template=feature_request.md&title=%5BREQ%5D">Request Feature</a>
  </p>
</div>

### Support My Work

I create free, open-source Counter-Strike 2 plugins for the community. If you'd like to support my work, consider becoming a sponsor!

#### 💖 GitHub Sponsors

Support this project through [GitHub Sponsors](https://github.com/sponsors/K4ryuu) with flexible options:

- **One-time** or **monthly** contributions
- **Custom amount** - choose what works for you
- **Multiple tiers available** - from basic benefits to priority support or private project access

Every contribution helps me dedicate more time to development, support, and creating new features. Thank you! 🙏

<p align="center">
  <a href="https://github.com/sponsors/K4ryuu">
    <img src="https://img.shields.io/badge/sponsor-30363D?style=for-the-badge&logo=GitHub-Sponsors&logoColor=#EA4AAA" alt="GitHub Sponsors" />
  </a>
</p>

⭐ **Or support me for free by starring this repository!**

### Dependencies

- [**SwiftlyS2**](https://github.com/swiftly-solution/swiftlys2): Server plugin framework for Counter-Strike 2
- **Database**: One of the following supported databases:
  - **MySQL / MariaDB** - Recommended for production
  - **PostgreSQL** - Full support
  - **SQLite** - Great for single-server setups

<p align="right">(<a href="#readme-top">back to top</a>)</p>

<!-- INSTALLATION -->

## Installation

1. Install [SwiftlyS2](https://github.com/swiftly-solution/swiftlys2) on your server
2. Configure your database connection in SwiftlyS2's `database.jsonc` (MySQL, PostgreSQL, or SQLite)
3. [Download the latest release](https://github.com/K4ryuu/K4-Seasons-SwiftlyS2/releases/latest)
4. Extract to your server's `swiftlys2/plugins/` directory
5. Configure `config.json`, `missions.json`, and `season_1.json` in the plugin folder
6. Restart your server - database tables and first season will be created automatically

<p align="right">(<a href="#readme-top">back to top</a>)</p>

<!-- FEATURES -->

## Features

### 🎯 Core Systems

#### Season System
- **Time-Limited Seasons**: Configure custom duration (e.g., 90 days)
- **Automatic Transitions**: Seamless season rollover with data preservation
- **Season-Specific Rewards**: Custom level rewards per season
- **Season History**: Track all past seasons and player achievements

#### XP & Leveling
- **Multiple XP Sources**: Earn experience from:
  - Kills (25 XP default)
  - Assists (tracked automatically)
  - MVPs (75 XP default)
  - Bomb plants (75 XP default)
  - Bomb defuses (150 XP default)
  - Hostage rescues (100 XP default)
  - Round wins (150 XP default)
  - Game wins (500 XP default)
- **Dynamic Level Scaling**: Optional progressive XP requirements
- **Farm Protection**: Minimum player count requirement (4 default)
- **Warmup Protection**: Optional XP disable during warmup
- **Progress Tracking**: Show XP progress on death or every N minutes

#### Battle Pass (Premium Tier)
- **Higher Level Cap**: 100 levels vs 20 for free players
- **XP Multiplier**: 1.5x XP bonus (configurable)
- **Extra Daily Missions**: 2 additional missions (configurable)
- **Extra Rerolls**: 2 additional mission rerolls per day
- **Exclusive Rewards**: Battle Pass-only level rewards
- **Admin Management**: Admins can grant Battle Pass via command

### 📋 Mission System

#### Daily Personal Missions
- **Random Assignment**: Players get 3-5 daily missions (3 base + 2 BP bonus)
- **Mission Rerolls**: 1-3 rerolls per day (1 base + 2 BP + 1 VIP)
- **Custom Objectives**: Create missions from ANY CS2 game event
- **Event Property Filters**: Filter by weapon, headshot, noscope, map, etc.
- **XP Rewards**: Configurable experience rewards per mission
- **Daily Reset**: Automatic midnight UTC reset with new assignments
- **Progress Tracking**: Real-time progress updates
- **Anti-Frustration System**: Detect struggling players and offer mission abandonment

#### Community Weekly Missions
- **Server-Wide Goals**: Everyone contributes to shared objectives
- **Weekly Rewards**: All players earn XP when community mission completes
- **Configurable Count**: 2 weekly missions (configurable)
- **Weekly Reset**: Automatic Sunday midnight reset

#### Mission Abandonment
- **Anti-Frustration Detection**: Automatic detection after 60 minutes of playtime
- **Partial XP Reward**: Earn XP based on progress percentage
- **Smart Hints**: System suggests abandonment for stuck missions
- **Command**: `!abandon <number>` - Only works for offered missions
- **Progress Calculation**: `(Progress / Required) * Mission XP`

### 🏆 Progression Systems

#### Prestige System
- **Prestige Levels**: Reset and climb with permanent bonuses (5 levels default)
- **XP Multiplier**: 1.2x per prestige level (stacks multiplicatively)
- **Requirements**: Must reach maximum level before prestiging
- **Reminders**: Optional periodic reminders when eligible
- **Command**: `!prestige` - Reset to level 1 with multiplier bonus

#### Streak System
- **Daily Streak Tracking**: Consecutive days of mission completion
- **Streak Requirements**: Complete 3 daily missions to maintain (configurable)
- **Tiered Bonuses**:
  - 3 days: 1.1x XP multiplier
  - 5 days: 1.25x XP multiplier
  - 7 days: 1.5x XP multiplier
- **Progress Display**: Shows current streak in profile

### 📊 Multiplier Stacking

**7 Independent Multiplier Sources** that stack multiplicatively:

| Multiplier | Description | Default Value | Notes |
|------------|-------------|---------------|-------|
| **Battle Pass** | Premium tier bonus | 1.5x | Permanent while BP active |
| **Prestige** | Per prestige level | 1.2x per level | P5 = 2.49x total |
| **Streak** | Daily mission completion | 1.1x - 1.5x | Requires 3 missions/day |
| **Weekly Bonus** | Day-of-week bonus | 1.0x - 1.2x | Weekend bonus default |
| **Prime Time** | Time-based bonus | 1.1x | 20:00-08:00 default |
| **Catch-Up** | Late-season help | 1.2x per week | BP only, scales with time |
| **Toplist** | Leaderboard position | 1.01x - 1.10x | Top 10 players |
| **VIP** | Server VIP players | 1.25x | Permission-based |

**Example Maximum Stack**: BP (1.5x) × P5 (2.49x) × Streak (1.5x) × Weekend (1.2x) × Prime (1.1x) × Catch-Up (2.49x at week 5) × Top1 (1.1x) × VIP (1.25x) = **~9.5x XP multiplier**

### 🎁 Custom Level Rewards

- **Commands**: Execute any server command at specific levels
- **Permissions**: Grant SwiftlyS2 permissions automatically
- **Groups**: Add players to permission groups
- **Battle Pass Exclusive**: Some rewards only for BP holders
- **Placeholders**: Use `{name}`, `{steamid64}`, `{userid}`, `{slot}` in commands
- **Per-Season Config**: Different rewards for each season

### 🏅 Other Features

#### Toplist System
- **Cached Leaderboard**: Updates every 5 minutes (optimized queries)
- **Position Multipliers**: Top 10 players get XP bonuses (1.01x - 1.10x)
- **Rank Display**: `!seasons` menu shows your rank
- **XP Sorting**: Ranked by total season XP

#### VIP System
- **Permission-Based**: Configure VIP flags in config
- **XP Multiplier**: 1.25x bonus (configurable)
- **Extra Rerolls**: +1 daily mission reroll
- **Automatic Detection**: Checks player permissions on join

#### Catch-Up Mechanic
- **Late-Season Help**: Players who join late get XP boost
- **Battle Pass Only**: Exclusive to BP holders
- **Week-Based Scaling**: 1.2x per week since season start
- **Automatic Calculation**: No admin intervention needed

#### Weekly Bonus
- **Day-of-Week Multipliers**: Different XP rates per day
- **Weekend Boost**: Saturday/Sunday 1.2x default
- **Fully Configurable**: Set any multiplier for any day
- **UTC-Based**: Uses server UTC time

#### Prime Time
- **Time-Based Bonuses**: Higher XP during configured hours
- **Cross-Midnight Support**: e.g., 20:00-08:00 works correctly
- **Configurable Hours**: HH:mm format (24-hour)
- **Multiplier**: 1.1x default

#### Data Management
- **Auto-Purge**: Remove inactive players after N days (90 default, 0=never)
- **Hot Reload**: Configuration changes without restart
- **Round-End Save**: Optional save all players each round
- **Session Persistence**: Data saved on disconnect

<p align="right">(<a href="#readme-top">back to top</a>)</p>

<!-- CONFIGURATION -->

## Configuration

### Database Settings

| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| `Connection` | string | `"default"` | Database connection name from SwiftlyS2's database.jsonc |
| `PurgeDays` | int | `90` | Days to keep inactive player records (0 = forever) |

### General Settings

| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| `DateFormat` | string | `"yyyy/MM/dd"` | Date format used in chat messages |
| `RoundEndSave` | bool | `true` | Save all player data at the end of each round |

### Command Settings

Each command has these configurable properties:

| Property | Type | Description |
|----------|------|-------------|
| `Command` | string | Main command name |
| `Aliases` | string[] | Alternative command names |
| `Permission` | string | Required permission (admin commands only) |

**Available Commands:**

| Command | Default | Aliases | Permission | Description |
|---------|---------|---------|------------|-------------|
| `Season` | `"seasons"` | `["season"]` | None | Open season menu |
| `Prestige` | `"prestige"` | `[]` | None | Prestige up |
| `GiveBattlePass` | `"givebp"` | `[]` | `"k4-seasons.admin"` | Admin: Grant Battle Pass |
| `RerollMission` | `"reroll"` | `[]` | None | Reroll a daily mission |
| `AbandonMission` | `"abandon"` | `[]` | None | Abandon a mission (anti-frustration only) |

### Experience Settings

| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| `EventsEnabled` | bool | `true` | Enable built-in XP reward events |
| `WarmupExperience` | bool | `true` | Award XP during warmup rounds |
| `ShowProgressOnDeath` | bool | `true` | Show XP progress when player dies |
| `ShowProgressOnMinutes` | int | `0` | Show XP progress every N minutes (0 = disabled) |
| `MinPlayerCount` | int | `4` | Minimum connected players for XP rewards (prevents farming) |
| `MvpReward` | int | `75` | XP reward for round MVP |
| `HostageRescueReward` | int | `100` | XP reward for rescuing a hostage |
| `BombDefuseReward` | int | `150` | XP reward for defusing the bomb |
| `BombPlantReward` | int | `75` | XP reward for planting the bomb |
| `KillReward` | int | `25` | XP reward for getting a kill |
| `RoundWinReward` | int | `150` | XP reward for winning a round |
| `GameWinReward` | int | `500` | XP reward for winning the match |

### Level Settings

| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| `ExperiencePerLevel` | int | `1500` | Base XP required per level |
| `ExperienceDynamicMultiplier` | float | `1.0` | XP scaling multiplier per level (1.0 = flat, >1.0 = progressive) |
| `LevelCap` | int | `20` | Maximum level for non-Battle Pass players |

### Mission Settings

| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| `RecordWarmup` | bool | `true` | Track mission progress during warmup |
| `MinPlayerCount` | int | `4` | Minimum connected players for mission progress |
| `DailyMissionCount` | int | `3` | Number of daily personal missions assigned |
| `WeeklyMissionCount` | int | `2` | Number of weekly community missions |
| `DailyRerollCount` | int | `1` | Daily mission reroll count |
| `EventDebugLogs` | bool | `false` | Log mission event details for debugging |

### Battle Pass Settings

| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| `Multiplier` | float | `1.5` | XP multiplier for Battle Pass holders |
| `DailyMissionCount` | int | `2` | Extra daily missions for Battle Pass holders |
| `MissionRerollCount` | int | `2` | Extra daily rerolls for Battle Pass holders |
| `LevelCap` | int | `100` | Maximum level for Battle Pass holders |

### Prestige Settings

| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| `Enabled` | bool | `false` | Enable prestige system |
| `LevelCap` | int | `5` | Maximum prestige level |
| `MultiplierPerLevel` | float | `1.2` | XP multiplier bonus per prestige level |
| `ReminderMinuteInterval` | int | `10` | Minutes between prestige reminder messages (0 = disabled) |

### VIP Settings

| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| `Flags` | string[] | `[]` | Permission flags that grant VIP status (any one of these) |
| `Multiplier` | float | `1.25` | XP multiplier for VIP players |
| `ExtraRerolls` | int | `1` | Extra daily mission rerolls for VIP players |

### Streak Settings

| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| `Enabled` | bool | `true` | Enable daily streak XP multiplier |
| `RequiredDailyComplete` | int | `3` | Completed daily missions needed to maintain streak |
| `DaySettings` | object | See below | Streak day thresholds and their XP multipliers |

**Default DaySettings:**
```json
{
  "3": 1.1,
  "5": 1.25,
  "7": 1.5
}
```

### Weekly Bonus Settings

| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| `Enabled` | bool | `false` | Enable day-of-week XP multipliers |
| `DayMultipliers` | object | See below | Day name and its XP multiplier |

**Default DayMultipliers:**
```json
{
  "Monday": 1.0,
  "Tuesday": 1.0,
  "Wednesday": 1.0,
  "Thursday": 1.0,
  "Friday": 1.0,
  "Saturday": 1.2,
  "Sunday": 1.2
}
```

### Prime Time Settings

| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| `Enabled` | bool | `false` | Enable time-based XP multiplier |
| `Start` | string | `"20:00"` | Prime time start (HH:mm format, 24-hour) |
| `End` | string | `"08:00"` | Prime time end (HH:mm format, 24-hour) |
| `Multiplier` | float | `1.1` | XP multiplier during prime time |

### Catch-Up Settings

| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| `Enabled` | bool | `false` | Enable catch-up XP bonus for Battle Pass holders behind the average |
| `MultiplierPerWeek` | float | `1.2` | XP multiplier bonus per week behind average |

### Toplist Settings

| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| `Enabled` | bool | `true` | Enable XP multipliers for top-ranked players |
| `PositionMultipliers` | object | See below | Leaderboard position and its XP multiplier |

**Default PositionMultipliers:**
```json
{
  "1": 1.10,
  "2": 1.09,
  "3": 1.08,
  "4": 1.07,
  "5": 1.06,
  "6": 1.05,
  "7": 1.04,
  "8": 1.03,
  "9": 1.02,
  "10": 1.01
}
```

### Anti-Frustration Settings

| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| `Enabled` | bool | `false` | Enable anti-frustration mission abandon hints |
| `MinuteInterval` | int | `60` | Minimum active time in minutes before checking for frustration |
| `BetweenDelay` | int | `15` | Cooldown in minutes between frustration checks per player |

<p align="right">(<a href="#readme-top">back to top</a>)</p>

## Season Configuration

Season configuration files define the rewards for each season. Place them in the plugin's resource directory as `season_1.json`, `season_2.json`, etc.

### Season File Structure

```json
{
  "seasonName": "Season 1: Winter",
  "durationDays": 90,
  "rewards": {
    "1": {
      "name": "Beginner",
      "commands": ["say {name} reached level 1!"],
      "permissions": [],
      "groups": [],
      "battlePassOnly": false
    },
    "50": {
      "name": "BP Silver",
      "commands": [
        "say {name} reached BP level 50!",
        "sw_addpermission {steamid64} vip.silver"
      ],
      "permissions": ["vip.silver"],
      "groups": ["silver_tier"],
      "battlePassOnly": true
    }
  }
}
```

### Season Fields

| Field | Type | Description |
|-------|------|-------------|
| `seasonName` | string | Display name for the season |
| `durationDays` | int | Season length in days |
| `rewards` | object | Level rewards (key = level number) |

### Level Reward Fields

| Field | Type | Description |
|-------|------|-------------|
| `name` | string | Reward tier name |
| `commands` | string[] | Server commands executed on reaching level |
| `permissions` | string[] | SwiftlyS2 permissions granted to the player |
| `groups` | string[] | Permission groups the player is added to |
| `battlePassOnly` | boolean | Whether reward requires Battle Pass |

### Reward Command Placeholders

| Placeholder | Description |
|-------------|-------------|
| `{steamid64}` | Player's Steam ID 64 |
| `{steamid}` | Same as steamid64 |
| `{name}` | Player's name |
| `{userid}` | Player's user ID |
| `{slot}` | Player's slot |

<p align="right">(<a href="#readme-top">back to top</a>)</p>

## Mission Configuration

Missions are defined in `missions.json`. The plugin supports **all CS2 game events** dynamically.

### Mission Definition Fields

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `Event` | string | Yes | Game event type to track (e.g., `EventPlayerDeath`) |
| `Target` | string | Yes | Event field identifying the player (e.g., `Attacker`, `Userid`) |
| `Type` | string | Yes | `Personal` (daily per-player) or `Community` (weekly server-wide) |
| `Name` | string | Yes | Mission description shown to players |
| `AmountToComplete` | int | Yes | Required count to complete the mission |
| `RewardExperience` | int | Yes | XP awarded on completion |
| `BattlePassOnly` | bool | Yes | Whether mission requires Battle Pass |
| `EventProperties` | object | No | Filter conditions for the event (weapon, headshot, etc.) |
| `Map` | string | No | Restrict mission to specific map |

### Available Events

The plugin supports **all CS2 game events**. Check the [CS2 Game Events List](https://cs2.poggu.me/dumped-data/game-events/) for complete documentation.

**Common events for missions:**

| Event | Target | Description |
|-------|--------|-------------|
| `EventPlayerDeath` | `Attacker`, `Assister` | Player kills/assists |
| `EventRoundMvp` | `Userid` | MVP awards |
| `EventBombPlanted` | `Userid` | Bomb plants |
| `EventBombDefused` | `Userid` | Bomb defuses |
| `EventHostageRescued` | `Userid` | Hostage rescues |
| `EventGrenadeThrown` | `Userid` | Grenade throws |
| `EventRoundEnd` | `winner`, `loser` | Round wins/losses |
| `PlayTime` | `Userid` | Minutes played (internal event) |

> **Note:** Event names must use PascalCase with `Event` prefix (e.g., `player_death` → `EventPlayerDeath`). The `Target` field should match a player-related property from the event.

### Event Properties (EventProperties)

Event properties allow filtering missions to specific conditions. The plugin dynamically reads all properties from game events.

> **💡 Tip:** Enable `EventDebugLogs: true` in config.json to see all available properties and their values in the server console when events fire.

#### Common EventPlayerDeath Properties

| Property | Type | Description | Example Value |
|----------|------|-------------|---------------|
| `Weapon` | string | Weapon name used for the kill | `"ak47"`, `"awp"`, `"knife"` |
| `Headshot` | bool | Whether it was a headshot | `true`, `false` |
| `Penetrated` | int | Number of surfaces penetrated (wallbang) | `0`, `1`, `2` |
| `Noscope` | bool | Whether it was a noscope kill | `true`, `false` |
| `Thrusmoke` | bool | Whether killed through smoke | `true`, `false` |
| `Attackerblind` | bool | Whether attacker was flashed | `true`, `false` |
| `Distance` | float | Distance between attacker and victim | `500.0` |

#### Property Matching Logic

- **String properties**: Uses case-insensitive contains matching (e.g., `"ak"` matches `"ak47"`)
- **Boolean properties**: Must match exactly (`true` or `false`)
- **Number properties**: Event value must be >= mission value (useful for minimum distance, penetration count)

### Mission Examples

#### Basic Kill Mission
```json
{
  "Event": "EventPlayerDeath",
  "Target": "Attacker",
  "Type": "Personal",
  "Name": "Kill 10 players",
  "AmountToComplete": 10,
  "RewardExperience": 500,
  "BattlePassOnly": false
}
```

#### Weapon-Specific Mission
```json
{
  "Event": "EventPlayerDeath",
  "EventProperties": {
    "Weapon": "awp"
  },
  "Target": "Attacker",
  "Type": "Personal",
  "Name": "Get 5 AWP kills",
  "AmountToComplete": 5,
  "RewardExperience": 750,
  "BattlePassOnly": false
}
```

#### Headshot Mission
```json
{
  "Event": "EventPlayerDeath",
  "EventProperties": {
    "Weapon": "ak47",
    "Headshot": true
  },
  "Target": "Attacker",
  "Type": "Personal",
  "Name": "Get 5 AK47 headshot kills",
  "AmountToComplete": 5,
  "RewardExperience": 1000,
  "BattlePassOnly": false
}
```

#### Wallbang Mission
```json
{
  "Event": "EventPlayerDeath",
  "EventProperties": {
    "Penetrated": 1
  },
  "Target": "Attacker",
  "Type": "Personal",
  "Name": "Get 3 wallbang kills",
  "AmountToComplete": 3,
  "RewardExperience": 1500,
  "BattlePassOnly": true
}
```

#### Community Mission
```json
{
  "Event": "EventPlayerDeath",
  "Target": "Attacker",
  "Type": "Community",
  "Name": "Community: 500 total kills",
  "AmountToComplete": 500,
  "RewardExperience": 200,
  "BattlePassOnly": false
}
```

#### Play Time Mission
```json
{
  "Event": "PlayTime",
  "Target": "Userid",
  "Type": "Personal",
  "Name": "Play for 60 minutes",
  "AmountToComplete": 60,
  "RewardExperience": 300,
  "BattlePassOnly": false
}
```

<p align="right">(<a href="#readme-top">back to top</a>)</p>

## Database

The plugin uses automatic schema management with FluentMigrator. Tables are created automatically on first run.

### Supported Databases

| Database | Status | Notes |
|----------|--------|-------|
| MySQL / MariaDB | ✅ Full | Recommended for multi-server setups |
| PostgreSQL | ✅ Full | Alternative for existing Postgres setups |
| SQLite | ✅ Full | Perfect for single-server, no setup needed |

### Database Tables

- `k4se_players` - Player progression data (XP, level, prestige, streak, battle pass status)
- `k4se_personal_missions` - Individual daily mission assignments and progress
- `k4se_community_missions` - Server-wide weekly community missions
- `k4se_seasons` - Season history and metadata

### Data Purging

Configure `PurgeDays` in Database settings:
- `0` - Keep all player data forever
- `90` - Remove players inactive for 90+ days (default)
- Any positive number - Custom retention period

<p align="right">(<a href="#readme-top">back to top</a>)</p>

## Commands

### Player Commands

| Command | Usage | Description |
|---------|-------|-------------|
| `!seasons` | `!seasons` | Open season menu with profile, missions, and rewards |
| `!season` | `!season` | Alias for !seasons |
| `!prestige` | `!prestige` | Prestige to next level (requires max level) |
| `!reroll` | `!reroll <number>` | Reroll a daily mission (e.g., `!reroll 2` for mission #2) |
| `!abandon` | `!abandon <number>` | Abandon a mission offered by anti-frustration system |

### Admin Commands

| Command | Permission | Usage | Description |
|---------|------------|-------|-------------|
| `!givebp` | `k4-seasons.admin` | `!givebp <name\|steamid>` | Grant Battle Pass to a player |

<p align="right">(<a href="#readme-top">back to top</a>)</p>

<!-- LICENSE -->

## License

Distributed under the GPL-3.0 License. See [`LICENSE.md`](LICENSE.md) for more information.

<p align="right">(<a href="#readme-top">back to top</a>)</p>

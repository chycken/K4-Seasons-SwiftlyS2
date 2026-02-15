# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [v1.0.1] - 2026-02-15

### Fixed

- Fixed player language detection by delaying welcome and mission assignment messages to ensure `cl_language` cvar is set before sending localized messages ([#1](https://github.com/K4ryuu/K4-Seasons-SwiftlyS2/issues/1))
- Fixed `ObjectDisposedException` on player disconnect by caching player username instead of accessing disposed player object during database save ([#2](https://github.com/K4ryuu/K4-Seasons-SwiftlyS2/issues/2))

## [v1.0.0] - 2026-02-13

### Added

- Initial SwiftlyS2 port of K4-Seasons battle pass system
- Season management with configurable duration and automatic transitions
- XP/leveling system with 20 free levels and 100 battle pass levels
- Battle pass with premium rewards and XP multipliers
- Personal daily missions with configurable count
- Community weekly missions with shared progress
- Prestige system with XP multiplier bonuses
- Streak system for consecutive daily mission completions
- Weekly day-based XP multipliers
- Prime time XP multiplier windows
- Catch-up XP bonus for late battle pass purchasers
- Top 10 leaderboard with XP bonus multipliers
- Anti-frustration system for stuck missions
- Level rewards with commands, permissions, and groups
- Discord webhook notifications
- Multi-database support (MySQL, MariaDB, PostgreSQL, SQLite)
- Automatic schema management with FluentMigrator
- Hot-reload configuration support

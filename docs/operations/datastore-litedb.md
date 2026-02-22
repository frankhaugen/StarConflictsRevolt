# LiteDB for game persistence

Sessions and clients are persisted with **LiteDB** (embedded, single-file NoSQL). **RavenDB** is still used for the event store (see [aspire.md](aspire.md)).

## Why LiteDB

- **Single file** – no separate DB server or migrations (e.g. `game.db` in the app directory).
- **Good .NET support** – [LiteDB](https://www.litedb.org/) NuGet, simple API.
- **UI** – [LiteDB Studio](https://github.com/mbdavid/LiteDB.Studio) (free) to browse and edit collections.
- **Schema flexibility** – add fields without migrations; no SQL/EF for this data.

## What is stored where

| Data            | Store     | Notes                                      |
|-----------------|-----------|--------------------------------------------|
| Sessions        | LiteDB    | `sessions` collection (Id, SessionName, PlayerId, …) |
| Clients         | LiteDB    | `clients` collection (Id, LastSeen, IsActive)        |
| Game events     | RavenDB   | Event store (IEventStore); replay per world          |

World state is in-memory (SessionAggregateManager); it is rebuilt from RavenDB events on load.

## Configuration

- **Connection string:** `ConnectionStrings__liteDb` or `LiteDb:FileName`.
- **Default:** `Filename=game.db` (current working directory when the WebApi runs).
- To use a specific path: set `ConnectionStrings__liteDb=Filename=C:\data\game.db` (or `LiteDb:FileName=...`) in appsettings or environment.

No Aspire container is used for LiteDB; the WebApi opens the file directly.

## Browsing data (LiteDB Studio)

1. Install [LiteDB Studio](https://github.com/mbdavid/LiteDB.Studio/releases) (or use the portable build).
2. Stop the WebApi (so the file is not locked).
3. Open `game.db` (in the WebApi run directory or the path you configured).
4. Inspect or edit the `sessions` and `clients` collections.

## Player stats (leaderboard)

Leaderboard data is stored in LiteDB in the `playerstats` collection. `IGamePersistence` exposes `GetLeaderboardAsync`, `GetPlayerStatsAsync`, and `GetTopPlayersAsync`. Nothing in the app currently writes `PlayerStats`; those methods return empty/list from LiteDB until a projection or service populates them.

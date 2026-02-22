# Storage Abstractions

Repository and ID contracts for the server, with **zero routing infrastructure**: DI closed-generic overrides replace open-generic defaults per entity type.

## Package

`StarConflictsRevolt.Server.Storage.Abstractions` defines:

- Core contracts: `IHasId`, `IRepository<T>`, `IRepositoryFactory`, `IRepositoryProvider`
- Id generation: `IIdProvider`, `GuidV7IdProvider`
- Builder: `IStorageBuilder`, `AddStorage()`, `BindRepository<T, TProvider>()`

Provider packages (e.g. LiteDB, Blob) register an open-generic default and an `IRepositoryProvider`; type-specific bindings are closed-generic registrations only.

## Usage (composition root)

```csharp
services.AddStorage(opt =>
{
    opt.AddLiteDbProvider(o => o.DatabasePath = "data/game.db");
    opt.AddBlobProvider(o => o.BasePath = "data/blobs");

    // Override only this type to blob-backed
    opt.BindRepository<WorldState, BlobRepositoryProvider>();
});
```

- `IRepository<AnythingElse>` → default (e.g. LiteDB open generic)
- `IRepository<WorldState>` → blob provider (closed generic override)
- `IRepositoryFactory.Create<T>()` resolves `IRepository<T>`; DI applies the override when present.

## Ordering

Register the **default open-generic** first, then **closed-generic overrides**. The builder call order in `AddStorage` does that when providers are added before `BindRepository` calls.

## LiteDB provider

`StarConflictsRevolt.Server.Storage.LiteDb` provides:

- **LiteDbOptions** – `DatabasePath`, `Password`, `WriteLockStripes`
- **AddLiteDbProvider(builder, configure)** – registers options, `ILiteDatabase` singleton, `IRepositoryProvider`, and open generic `IRepository<>` → `LiteDbRepository<>`
- Per-id `SemaphoreSlim` write serialization inside `LiteDbRepository<T>`

The same `ILiteDatabase` is registered so existing consumers (e.g. `LiteDbGamePersistence`) keep working without change.

## JSON directory provider

`StarConflictsRevolt.Server.Storage.JsonFiles` provides:

- **Layout**: `RootPath/<TypeName>/<Id:N>.json` (Id = 32 hex chars, no dashes). Optional process lock: `RootPath/.store.lock`.
- **JsonFilesOptions** – `RootPath`, `ReadOnly`, `CreateIfMissing`, `UseProcessLock`, `LockStripes`.
- **AddJsonFilesProvider(builder, configure)** – runs fail-fast startup validation (root exists or created, enumeration; if not read-only: probe write/delete and atomic replace; optionally acquires `.store.lock`), then registers options, `IRepositoryProvider`, and open generic `IRepository<>` → `JsonFilesRepository<>`.
- **Serialization**: System.Text.Json only. Deserialization failure → record treated as missing; `All()` skips unreadable files.
- **Atomicity**: Write to `file.tmp`, flush to disk, `File.Move(tmp, final, overwrite: true)`. Delete = `File.Delete(final)`.
- **Concurrency**: Striped `SemaphoreSlim[]` keyed by (type, id); reads lock-free.
- **CreateAsync** (extension): `repo.CreateAsync(idProvider, id => new T(id, ...))` for generate-and-persist in one step.
- **Indexing**: None (by design).

## WebApi wiring

`StartupHelper.RegisterLiteDb(builder)` now uses `AddStorage` + `AddLiteDbProvider` (path from `ConnectionStrings:liteDb` or `LiteDb:FileName`, default `game.db`), then registers `IGamePersistence` → `LiteDbGamePersistence`. Session/client/leaderboard behaviour is unchanged.

## Event storage (RavenDB)

Event persistence is in a separate abstraction and implementation:

- **StarConflictsRevolt.Server.EventStorage.Abstractions** – `IEventStore`, `IGameEvent`, `EventEnvelope` (contract only; `IGameEvent.ApplyTo(object world, ILogger)` so the abstraction does not reference `World`).
- **StarConflictsRevolt.Server.EventStorage.RavenDB** – `RavenEventStore` (channel + process loop, persist to RavenDB, dispatch to subscribers), `RavenDbEventStorageOptions`, `AddRavenDbEventStorage(services, configuration)`. Also registers `IDocumentStore` and ensures the database exists. `GetEventsForWorld` and `SnapshotWorld` remain on `RavenEventStore` for replay/snapshot.

WebApi calls `RegisterRavenDb(builder)` (which uses `AddRavenDbEventStorage(builder.Configuration)`) **before** `RegisterAllServices` so `IEventStore` is available. Concrete game events (e.g. `MoveFleetEvent`) stay in WebApi and implement the Abstractions `IGameEvent` with `ApplyTo(object world, ILogger)` casting `world` to `World.World`.

## See also

- [datastore-litedb.md](../operations/datastore-litedb.md) for LiteDB usage
- [architecture.md](architecture.md) for pipeline and event store

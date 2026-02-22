# Aspire AppHost

Orchestration for **local development**: dashboard, webapi, Blazor, and (optionally) containerized Redis, SQL Server, RavenDB. Used for running and debugging the full stack; not required for building the solution. For build/test and code layout see [development.md](development.md).

## Running

From solution root:

```bash
dotnet run --project StarConflictsRevolt.Aspire.AppHost
```

**Docker required.** Redis, SQL Server, and RavenDB run as containers. Start Docker Desktop (and ensure it is healthy) before running the AppHost, or the database containers will not start and the webapi will log warnings and may fail on first DB use. The console prints the dashboard URL; open it to see resources, health, and logs.

**Build note:** If you build the solution while the AppHost is running, the build can fail with “file is locked” (AppHost exe). Stop the AppHost process, then run `dotnet build StarConflictsRevolt.slnx` again.

## When the dashboard shows issues

| What you see | What to do |
|--------------|------------|
| **Build fails: file is locked / MSB3027** | Stop the running AppHost (close its window or end the process), then build again. |
| **Containers (redis, gameDb, ravenDb) unhealthy or not starting** | Start Docker Desktop and ensure it is healthy; restart the AppHost. |
| **webapi or blazor unhealthy** | Check that the project exposes `/health`; check dashboard logs for the failing project. |
| **NU1510 package warnings in Blazor** | Redundant package references were removed; remove any that are already implied by the SDK. |

## Resources

| Resource | Purpose |
|----------|---------|
| **redis** | Caching/session; persistent volume `redis-data`, parameterized password. |
| **gameDb** | SQL Server (EF Core game data); persistent volume `gameDb-data`, parameterized password. |
| **ravenDb** | RavenDB (event store); persistent volume `ravenDb-data`, unsecured for dev. |

All use `ContainerLifetime.Persistent` so data survives AppHost restarts.

**Don't overgenerate containers:** Containers are only started when `Aspire:UseContainers` is true (default). Set `Aspire:UseContainers` to `false` (e.g. in appsettings, user secrets, or env `Aspire__UseContainers=false`) to use existing Redis/SQL Server/RavenDB instances: the AppHost will not start any Docker containers and will pass connection strings from parameters `redis-connection`, `gamedb-connection`, `ravendb-connection` (override via config or env).

## Projects

- **webapi** — `StarConflictsRevolt.Server.WebApi`. References redis, gameDb, ravenDb; waits for all three and exposes HTTP. Health check: `GET /health` on the `http` endpoint.
- **blazor** — `StarConflictsRevolt.Clients.Blazor`. References webapi; waits for webapi. Health check: `GET /health` on the `http` endpoint. Blazor calls `MapDefaultEndpoints()` so `/health` is available in development.

## Client configuration (Blazor)

Set by AppHost so the Blazor app talks to the right backend (all use the webapi HTTP endpoint):

| Env var | Value |
|---------|--------|
| `GameClientConfiguration__ApiBaseUrl` | webapi HTTP endpoint (REST, token). |
| `GameClientConfiguration__GameServerUrl` | Same as ApiBaseUrl (server base URL). |
| `GameClientConfiguration__GameServerHubUrl` | webapi base + `/gamehub` (WorldHub). |
| `GameClientConfiguration__CommandHubUrl` | webapi base + `/commandhub` (GameHub for commands). |
| `GameClientConfiguration__TokenEndpoint` | webapi base + `/token`. |
| `TokenProviderOptions__TokenEndpoint` | webapi base + `/token` (auth token endpoint). |

When running under Aspire, the client can use `CommandHubUrl` for sending commands via SignalR (GameHub) in addition to WorldHub for world updates.

## Health checks

- **webapi**: `WithHttpHealthCheck(path: "/health", endpointName: "http")` — dashboard shows health and startup order can wait on it.
- **blazor**: same; Blazor must call `MapDefaultEndpoints()` so `/health` is mapped in development.

## Parameters

- **redis-password** — default `My!Password123`; override via user secrets or host configuration.
- **sqlserver-password** — default `My!Password123`; override similarly.

## Optional tuning

- **Resource limits**: On container resources (`redis`, `gameDb`, `ravenDb`), add `.WithMemoryLimit(...)` when supported by the hosting package to cap memory.
- **OTLP**: When the dashboard runs the AppHost, it injects `OTEL_EXPORTER_OTLP_ENDPOINT`; ServiceDefaults in webapi and blazor send traces and metrics there.
- **RavenDB Studio**: If the RavenDB container exposes a management UI, add `.WithUrl("studio", ...)` so the dashboard shows a direct link.
- **Custom dashboard commands**: `.WithCommand("open-api", "Open API", ...)` on the webapi project adds a dashboard button to open the API base URL.
- **Fixed ports**: `.WithEndpoint(..., port: 5153)` (or similar) on projects for stable URLs when debugging or using external tools.

---

## See also

- [development.md](development.md) — Build, test, run; running without AppHost.
- [../reference/architecture.md](../reference/architecture.md) — What webapi and Blazor do in the pipeline.

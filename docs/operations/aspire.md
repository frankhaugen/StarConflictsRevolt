# Aspire AppHost

Orchestration for **local development**: dashboard, webapi, Blazor, and (optionally) containerized Redis and RavenDB. Used for running and debugging the full stack; not required for building the solution. For build/test and code layout see [development.md](development.md).

## Running

From solution root:

```bash
dotnet run --project StarConflictsRevolt.Aspire.AppHost
```

Or use the recommended command from [AGENTS.md](../../AGENTS.md): `aspire run`.

By default the AppHost **does not use containers** (`Aspire:UseContainers` defaults to false). WebApi uses connection strings for Redis and RavenDB (parameters/local instances). To use Docker containers for Redis and RavenDB, set `Aspire:UseContainers` to `true` (e.g. in launchSettings or user secrets); then start Docker Desktop before running the AppHost. The console prints the dashboard URL; open it to see resources, health, and logs.

**Build note:** If you build the solution while the AppHost is running, the build can fail with “file is locked” (AppHost exe). Stop the AppHost process, then run `dotnet build StarConflictsRevolt.slnx` again.

## When the dashboard shows issues

| What you see | What to do |
|--------------|------------|
| **Build fails: file is locked / MSB3027** | Stop the running AppHost (close its window or end the process), then build again. |
| **Containers (redis, ravenDb) unhealthy or not starting** | Start Docker Desktop and ensure it is healthy; restart the AppHost. |
| **webapi or blazor unhealthy** | Check that the project exposes `/health`; check dashboard logs. If webapi is unhealthy, check RavenDB (webapi registers a RavenDB dependency health check). |
| **NU1510 package warnings in Blazor** | Redundant package references were removed; remove any that are already implied by the SDK. |

## Resources

| Resource | Purpose |
|----------|---------|
| **redis** | Caching/session (optional); persistent volume `redis-data`, parameterized password. Not yet used by WebApi for health checks. |
| **ravenDb** | RavenDB (event store); persistent volume `ravenDb-data`, unsecured for dev. Port set in AppHost (e.g. 5090). |

All use `ContainerLifetime.Persistent` so data survives AppHost restarts.

**Containers optional:** Containers are only started when `Aspire:UseContainers` is explicitly `true`. Default is no containers; the AppHost passes connection strings from parameters `redis-connection`, `ravendb-connection` (override via config or env).

## Projects

- **webapi** — `StarConflictsRevolt.Server.WebApi`. References redis and ravenDb when using containers; waits for them and exposes **HTTPS** (launch profile `https`). Health check: `GET /health` on the **https** endpoint. Registers a **RavenDB dependency health check** so the dashboard shows event-store readiness.
- **blazor** — `StarConflictsRevolt.Clients.Blazor`. References webapi; waits for webapi. Runs over **HTTPS** (launch profile `https`). Health check: `GET /health` on the **https** endpoint. Client configuration (API, hubs, token) uses the webapi HTTPS URL. Blazor calls `MapDefaultEndpoints()` so `/health` is available in development.

## Client configuration (Blazor)

Set by AppHost so the Blazor app talks to the right backend (all use the webapi **HTTPS** endpoint):

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

- **webapi**: `WithHttpHealthCheck(path: "/health", endpointName: "https")` — dashboard shows health and startup order can wait on it. WebApi also registers an application-level **RavenDB health check** (`ravendb`), so `/health` reflects both process and event-store readiness.
- **blazor**: same; Blazor must call `MapDefaultEndpoints()` so `/health` is mapped in development.

## Parameters

- **redis-password** — default `My!Password123`; override via user secrets or host configuration (when using containers).

## Optional tuning

- **Resource limits**: On container resources (`redis`, `ravenDb`), add `.WithMemoryLimit(...)` when supported by the hosting package to cap memory.
- **OTLP**: When the dashboard runs the AppHost, it injects `OTEL_EXPORTER_OTLP_ENDPOINT`; ServiceDefaults in webapi and blazor send traces and metrics there (no duplicate OpenTelemetry registration in app code).
- **RavenDB Studio**: If the RavenDB container exposes a management UI, add `.WithUrl("studio", ...)` so the dashboard shows a direct link.
- **Custom dashboard commands**: `.WithCommand("open-api", "Open API", ...)` on the webapi project adds a dashboard button to open the API base URL.
- **Fixed ports**: `.WithEndpoint(..., port: 5153)` (or similar) on projects for stable URLs when debugging or using external tools.
- **RavenDB port**: The AppHost sets `RavenDBServerSettings.Port` (e.g. 5090); use a free host port to avoid "bind: access forbidden".

## Improving the Aspire experience

| Improvement | Status / suggestion |
|-------------|----------------------|
| **Dependency health checks** | WebApi registers a RavenDB health check so the dashboard shows event-store readiness. Add Redis when WebApi uses Redis. |
| **Single OpenTelemetry setup** | ServiceDefaults provide logging, metrics, and tracing; webapi and blazor do not register duplicate OpenTelemetry. |
| **Use Aspire MCP** | Use _list resources_, _list traces_, _list console logs_ (see AGENTS.md) to inspect state and debug. |
| **Playwright for UI** | Use the Playwright MCP to drive the Blazor app from the dashboard endpoints (see playtest-runbook.md). |
| **Avoid persistent containers early** | Prefer non-persistent containers during early development to avoid state issues on restart (see AGENTS.md). |

---

## See also

- [development.md](development.md) — Build, test, run; running without AppHost.
- [playtest-runbook.md](playtest-runbook.md) — Assistant-driven playtest with Playwright MCP.
- [../reference/architecture.md](../reference/architecture.md) — What webapi and Blazor do in the pipeline.

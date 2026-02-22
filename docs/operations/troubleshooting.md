# Troubleshooting

Common issues when building, running, or playing StarConflictsRevolt, and how to fix them.

---

## Build

| Problem | Cause | What to do |
|---------|--------|------------|
| Build fails: file is locked (MSB3027) | Aspire AppHost is running and locking its exe. | Stop the AppHost. Then run `dotnet build StarConflictsRevolt.slnx` again. |
| Restore or build errors in one project | Missing SDK, package mismatch, or broken reference. | Run `dotnet restore StarConflictsRevolt.slnx` then build. Check target framework (e.g. net10.0). |
| NU1510 or other package warnings | Redundant or conflicting package references. | Remove duplicate references; see [aspire.md](aspire.md) for Blazor. |

---

## AppHost and containers

| Problem | Cause | What to do |
|---------|--------|------------|
| Containers (redis, gameDb, ravenDb) unhealthy or not starting | Docker not running or unhealthy; ports in use. | Start Docker Desktop; ensure healthy. Restart AppHost. Or set Aspire:UseContainers=false and provide connection strings ([aspire.md](aspire.md)). |
| **HTTP 500 – Port 8080 bind forbidden** (`listen tcp 127.0.0.1:8080: bind: An attempt was made to access a socket in a way forbidden by its access permissions`) | Windows reserves or blocks port 8080 (e.g. excluded port range, Hyper-V). | This repo uses **RavenDB on host port 8090** (not 8080). Pull latest and run AppHost again. If you use an older build, update AppHost to set `RavenDBServerSettings.Port = 8090` and connection fallbacks to `localhost:8090`; see [aspire.md](aspire.md). |
| webapi or blazor unhealthy in dashboard | App failed to start (config, DB connection). | Check dashboard logs for the failing project. Verify connection strings (gameDb, ravenDb, redis). |
| Connection string not set in webapi logs | Containers not started or UseContainers=false without parameters. | Run with Docker (default) or set ConnectionStrings in config/env and AppHost parameters (gamedb-connection, etc.). |

---

## Sessions and commands

| Problem | Cause | What to do |
|---------|--------|------------|
| Session not found / 404 on create or join | Session not in DB or aggregate not created. | Create with POST /game/session first. On join, aggregate is created if missing. |
| 404 on move-fleet | Invalid worldId or session aggregate not present. | Use the sessionId from create/join as worldId. Create then join so aggregate exists. |
| Command accepted but nothing in UI | Client not in WorldHub group or not listening for ReceiveUpdates. | Call JoinWorld(worldId) on WorldHub; subscribe to ReceiveUpdates and apply deltas. |
| No deltas received | Wrong SignalR group or no state change. | Verify JoinWorld(worldId). Check logs for CommandRejected. Ensure tick loop is running. |

---

## Auth and REST

| Problem | Cause | What to do |
|---------|--------|------------|
| 401 Unauthorized | Missing or invalid Bearer token. | Use POST /token; send Authorization: Bearer &lt;token&gt;. See [../reference/api-transport.md](../reference/api-transport.md). |
| CORS errors in browser | Blazor and webapi on different origins. | Ensure Blazor API base URL points at webapi. Aspire sets this via env. |

---

## Tests

| Problem | Cause | What to do |
|---------|--------|------------|
| TearDown or missing attribute | TUnit is used, not NUnit. | Do not use NUnit attributes; use TUnit lifecycle or fixture cleanup. |
| Integration test: connection or hub | Test host config. | Ensure test host registers hubs and URLs; see test project and [development.md](development.md). |

---

## See also

- [aspire.md](aspire.md) — When the dashboard shows issues.
- [../getting-started.md](../getting-started.md) — Create session, join, move fleet.
- [../reference/api-transport.md](../reference/api-transport.md) — Auth, hubs, REST.

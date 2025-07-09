using System.Numerics;
using Microsoft.AspNetCore.SignalR;
using StarConflictsRevolt.Clients.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StarConflictsRevolt.Server.Core;
using StarConflictsRevolt.Server.Eventing;
using System.Collections.Concurrent;

namespace StarConflictsRevolt.Server.GameEngine;

public class GameUpdateService : BackgroundService
{
    private readonly IHubContext<WorldHub> _hubContext;
    private readonly ILogger<GameUpdateService> _logger;
    // Session state and command queues
    private readonly ConcurrentDictionary<Guid, SessionAggregate> _aggregates = new();
    private readonly CommandQueue<IGameEvent> _commandQueue = new();

    public GameUpdateService(IHubContext<WorldHub> hubContext, ILogger<GameUpdateService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // For demo: create a single session/world
        var worldId = Guid.NewGuid();
        var initialWorld = new World(worldId, new Galaxy(Guid.NewGuid(), new List<StarSystem>()));
        var aggregate = new SessionAggregate(worldId, initialWorld);
        _aggregates[worldId] = aggregate;

        while (!stoppingToken.IsCancellationRequested)
        {
            foreach (var (sessionId, sessionAggregate) in _aggregates)
            {
                // Process all queued commands for this session
                while (_commandQueue.TryDequeue(sessionId, out var command))
                {
                    sessionAggregate.Apply(command);
                    // TODO: Persist event to RavenDB, update SQL projections, etc.
                }

                // Compute deltas (stub: use ChangeTracker)
                // TODO: Keep previous world state for diffing
                var deltas = ChangeTracker.ComputeDeltas(sessionAggregate.World, sessionAggregate.World);

                // Broadcast deltas to clients (stub)
                await _hubContext.Clients.Group(sessionId.ToString()).SendAsync("ReceiveUpdates", deltas, stoppingToken);
            }

            // Wait for next tick
            await Task.Delay(1000, stoppingToken); // 1 tick per second
        }
    }
}
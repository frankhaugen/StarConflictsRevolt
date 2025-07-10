using Microsoft.Extensions.Logging;
using StarConflictsRevolt.Server.Core.Models;

namespace StarConflictsRevolt.Server.Services;

public class SessionManagerService
{
    private readonly SessionAggregateManager _aggregateManager;
    private readonly ILogger<SessionManagerService> _logger;
    private readonly Dictionary<Guid, int> _eventCounts = new();
    private readonly Dictionary<Guid, World> _previousWorldStates = new();

    public SessionManagerService(SessionAggregateManager aggregateManager, ILogger<SessionManagerService> logger)
    {
        _aggregateManager = aggregateManager;
        _logger = logger;
    }

    public void CreateSession(Guid sessionId, World initialWorld)
    {
        _logger.LogInformation("Creating session {SessionId} with world {WorldId}", sessionId, initialWorld.Id);
        _aggregateManager.GetOrCreateAggregate(sessionId, initialWorld);
        _eventCounts[sessionId] = 0;
        _previousWorldStates[sessionId] = DeepCloneWorld(initialWorld);
    }

    public bool SessionExists(Guid worldId)
    {
        return _aggregateManager.HasAggregate(worldId);
    }

    public async Task<bool> SessionExistsAsync(Guid worldId)
    {
        return await Task.FromResult(SessionExists(worldId));
    }

    public int GetEventCount(Guid sessionId)
    {
        return _eventCounts.GetValueOrDefault(sessionId, 0);
    }

    public void IncrementEventCount(Guid sessionId)
    {
        _eventCounts[sessionId] = GetEventCount(sessionId) + 1;
    }

    public World? GetPreviousWorldState(Guid sessionId)
    {
        return _previousWorldStates.GetValueOrDefault(sessionId);
    }

    public void SetPreviousWorldState(Guid sessionId, World world)
    {
        _previousWorldStates[sessionId] = DeepCloneWorld(world);
    }

    private static World DeepCloneWorld(World world)
    {
        var options = new System.Text.Json.JsonSerializerOptions
        {
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles,
            WriteIndented = true,
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
        };
        var json = System.Text.Json.JsonSerializer.Serialize(world, options);
        return System.Text.Json.JsonSerializer.Deserialize<World>(json, options)!;
    }
}


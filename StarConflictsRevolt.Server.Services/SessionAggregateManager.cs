using System.Collections.Concurrent;
using StarConflictsRevolt.Server.Core.Models;
using StarConflictsRevolt.Server.Eventing;
using Microsoft.Extensions.Logging;

namespace StarConflictsRevolt.Server.Services;

/// <summary>
/// Manages SessionAggregate instances and provides a DI-friendly interface.
/// This is the service that should be injected, not SessionAggregate directly.
/// </summary>
public class SessionAggregateManager
{
    private readonly ConcurrentDictionary<Guid, SessionAggregate> _aggregates = new();
    private readonly IEventStore _eventStore;
    private readonly ILogger<SessionAggregateManager> _logger;

    public SessionAggregateManager(IEventStore eventStore, ILogger<SessionAggregateManager> logger)
    {
        _eventStore = eventStore;
        _logger = logger;
    }

    /// <summary>
    /// Gets or creates a SessionAggregate for the given session ID.
    /// </summary>
    public SessionAggregate GetOrCreateAggregate(Guid sessionId, World? initialWorld = null)
    {
        return _aggregates.GetOrAdd(sessionId, id =>
        {
            _logger.LogInformation("Creating new SessionAggregate for session {SessionId}", id);
            
            // Create initial world if not provided
            var world = initialWorld ?? new World(id, new Galaxy(Guid.NewGuid(), new List<StarSystem>()));
            
            var aggregate = new SessionAggregate(id, world);
            
            // Replay events from event store if available
            if (_eventStore is RavenEventStore ravenStore)
            {
                try
                {
                    var events = ravenStore.GetEventsForWorld(id).Select(e => e.Event);
                    aggregate.ReplayEvents(events);
                    _logger.LogInformation("Replayed {EventCount} events for session {SessionId}", events.Count(), id);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to replay events for session {SessionId}, starting with initial world", id);
                }
            }
            
            return aggregate;
        });
    }

    /// <summary>
    /// Gets an existing SessionAggregate for the given session ID.
    /// Returns null if not found.
    /// </summary>
    public SessionAggregate? GetAggregate(Guid sessionId)
    {
        return _aggregates.TryGetValue(sessionId, out var aggregate) ? aggregate : null;
    }

    /// <summary>
    /// Checks if a SessionAggregate exists for the given session ID.
    /// </summary>
    public bool HasAggregate(Guid sessionId)
    {
        return _aggregates.ContainsKey(sessionId);
    }

    /// <summary>
    /// Removes a SessionAggregate from memory (e.g., when session ends).
    /// </summary>
    public bool RemoveAggregate(Guid sessionId)
    {
        var removed = _aggregates.TryRemove(sessionId, out _);
        if (removed)
        {
            _logger.LogInformation("Removed SessionAggregate for session {SessionId}", sessionId);
        }
        return removed;
    }

    /// <summary>
    /// Gets all active session IDs.
    /// </summary>
    public IEnumerable<Guid> GetActiveSessionIds()
    {
        return _aggregates.Keys;
    }

    /// <summary>
    /// Gets all active SessionAggregates.
    /// </summary>
    public IEnumerable<SessionAggregate> GetAllAggregates()
    {
        return _aggregates.Values;
    }
} 
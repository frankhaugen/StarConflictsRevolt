using StarConflictsRevolt.Server.WebApi.Eventing;
using StarConflictsRevolt.Server.WebApi.Models;

namespace StarConflictsRevolt.Server.WebApi.Services;

public class SessionAggregate
{
    private readonly ILogger<SessionAggregate> _logger;

    public SessionAggregate(Guid sessionId, World initialWorld, ILogger<SessionAggregate> logger)
    {
        SessionId = sessionId;
        World = initialWorld;
        Version = 0;
        _logger = logger;
    }

    public Guid SessionId { get; set; }
    public World World { get; set; }
    public int Version { get; set; }
    public List<IGameEvent> UncommittedEvents { get; } = new();

    public void Apply(IGameEvent e)
    {
        _logger.LogInformation("Applying event {EventType} to session {SessionId}, version {Version}", e.GetType().Name, SessionId, Version);
        e.ApplyTo(World, _logger);
        UncommittedEvents.Add(e);
        Version++;
        _logger.LogInformation("Event {EventType} applied successfully to session {SessionId}, new version: {Version}",
            e.GetType().Name, SessionId, Version);
    }

    public void ReplayEvents(IEnumerable<IGameEvent> events)
    {
        foreach (var e in events)
            Apply(e);
        UncommittedEvents.Clear();
    }

    public void LoadFromSnapshot(World snapshot, int version)
    {
        World = snapshot;
        Version = version;
        UncommittedEvents.Clear();
    }
}
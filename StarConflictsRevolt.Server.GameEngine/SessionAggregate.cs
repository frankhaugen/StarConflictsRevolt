using StarConflictsRevolt.Server.Core;
using StarConflictsRevolt.Server.Eventing;

namespace StarConflictsRevolt.Server.GameEngine;

public class SessionAggregate
{
    public Guid SessionId { get; set; }
    public World World { get; set; }
    public int Version { get; set; }
    public List<IGameEvent> UncommittedEvents { get; } = new();

    public SessionAggregate(Guid sessionId, World initialWorld)
    {
        SessionId = sessionId;
        World = initialWorld;
        Version = 0;
    }

    public void Apply(IGameEvent e)
    {
        switch (e)
        {
            case MoveFleetEvent move:
                // TODO: Implement fleet movement logic
                break;
            case BuildStructureEvent build:
                // TODO: Implement structure building logic
                break;
            case AttackEvent attack:
                // TODO: Implement attack logic
                break;
            case DiplomacyEvent diplo:
                // TODO: Implement diplomacy logic
                break;
        }
        UncommittedEvents.Add(e);
        Version++;
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
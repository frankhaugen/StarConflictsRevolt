using StarConflictsRevolt.Clients.Models;
using StarConflictsRevolt.Clients.Raylib.Http;

namespace StarConflictsRevolt.Tests.TestingInfrastructure;

public class TestClientWorldStore : IClientWorldStore
{
    private WorldDto? _current;
    private readonly List<WorldDto?> _history = new();

    public IReadOnlyList<WorldDto?> History => _history.AsReadOnly();
    public SessionDto? Session { get; set; }

    public void ApplyFull(WorldDto? world)
    {
        _current = world;
        _history.Add(world);
    }

    public void ApplyDeltas(IEnumerable<GameObjectUpdate> deltas)
    {
        // Simplified implementation for testing
    }

    public WorldDto? GetCurrent() => _current;

    public void ClearHistory() => _history.Clear();
}
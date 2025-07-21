using StarConflictsRevolt.Clients.Models;

namespace StarConflictsRevolt.Clients.Shared.Infrastructure;

public interface IClientWorldStore
{
    IReadOnlyList<WorldDto?> History { get; }
    SessionDto? Session { get; set; }
    void ApplyFull(WorldDto? world);
    void ApplyDeltas(IEnumerable<GameObjectUpdate> deltas);
    WorldDto? GetCurrent();
}
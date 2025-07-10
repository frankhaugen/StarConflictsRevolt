using StarConflictsRevolt.Clients.Models;

namespace StarConflictsRevolt.Clients.Raylib.Http;

public interface IClientWorldStore
{
    void ApplyFull(WorldDto? world);
    void ApplyDeltas(IEnumerable<GameObjectUpdate> deltas);
    WorldDto? GetCurrent();
    IReadOnlyList<WorldDto?> History { get; }
    SessionDto? Session { get; set; }
}
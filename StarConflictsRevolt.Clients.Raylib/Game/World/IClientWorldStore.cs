using StarConflictsRevolt.Clients.Models;

namespace StarConflictsRevolt.Clients.Raylib.Game.World;

public interface IClientWorldStore
{
    IReadOnlyList<WorldDto?> History { get; }
    SessionDto? Session { get; set; }
    void ApplyFull(WorldDto? world);
    void ApplyDeltas(IEnumerable<GameObjectUpdate> deltas);
    WorldDto? GetCurrent();
}
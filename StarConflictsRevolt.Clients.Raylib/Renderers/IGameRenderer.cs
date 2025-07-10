using StarConflictsRevolt.Clients.Models;

namespace StarConflictsRevolt.Clients.Raylib.Renderers;

public interface IGameRenderer
{
    Task<bool> RenderAsync(WorldDto? world, CancellationToken cancellationToken);
}
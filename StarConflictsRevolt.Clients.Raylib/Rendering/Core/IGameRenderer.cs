using StarConflictsRevolt.Clients.Models;

namespace StarConflictsRevolt.Clients.Raylib.Rendering.Core;

public interface IGameRenderer
{
    Task<bool> RenderAsync(WorldDto? world, CancellationToken cancellationToken);
}
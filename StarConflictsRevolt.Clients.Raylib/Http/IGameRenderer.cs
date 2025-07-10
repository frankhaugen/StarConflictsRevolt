using StarConflictsRevolt.Clients.Models;

namespace StarConflictsRevolt.Clients.Raylib.Http;

public interface IGameRenderer
{
    Task<bool> RenderAsync(WorldDto? world, CancellationToken cancellationToken);
}
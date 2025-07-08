using StarConflictsRevolt.Clients.Models;

namespace StarConflictsRevolt.Clients.Shared;

public interface IGameRenderer
{
    Task<bool> RenderAsync(WorldDto? world, CancellationToken cancellationToken);
}
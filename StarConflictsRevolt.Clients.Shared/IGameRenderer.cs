namespace StarConflictsRevolt.Clients.Shared;

public interface IGameRenderer
{
    Task RenderAsync(WorldDto world, CancellationToken cancellationToken);
}
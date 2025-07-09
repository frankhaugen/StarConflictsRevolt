namespace StarConflictsRevolt.Server.Core.Models;

public abstract record GameObject : IGameObject
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
}
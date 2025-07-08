namespace StarConflictsRevolt.Server.Core;

public abstract record GameObject : IGameObject
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
}
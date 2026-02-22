namespace StarConflictsRevolt.Server.Domain.Gameplay;

public abstract record GameObject
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
}
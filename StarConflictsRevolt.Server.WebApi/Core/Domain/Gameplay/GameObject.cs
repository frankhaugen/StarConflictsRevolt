namespace StarConflictsRevolt.Server.WebApi.Core.Domain.Gameplay;

public abstract record GameObject
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
}
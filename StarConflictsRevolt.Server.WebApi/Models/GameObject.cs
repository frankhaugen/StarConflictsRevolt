namespace StarConflictsRevolt.Server.Core.Models;

public abstract record GameObject
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
}
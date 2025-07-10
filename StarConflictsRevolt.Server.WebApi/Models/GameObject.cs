namespace StarConflictsRevolt.Server.WebApi.Models;

public abstract record GameObject
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
}
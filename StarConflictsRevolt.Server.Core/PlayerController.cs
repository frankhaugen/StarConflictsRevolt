namespace StarConflictsRevolt.Server.Core;

public abstract record PlayerController
{
    public Guid PlayerId { get; init; }
}
namespace StarConflictsRevolt.Server.Core;

abstract record PlayerController
{
    public Guid PlayerId { get; init; }
}
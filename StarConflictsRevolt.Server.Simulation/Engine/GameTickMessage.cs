namespace StarConflictsRevolt.Server.Simulation.Engine;

/// <summary>
/// Domain-agnostic tick payload: tick number and timestamp. Used by Transport and listeners.
/// </summary>
public record GameTickMessage
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime Created { get; init; } = DateTime.UtcNow;
    public required GameTickNumber TickNumber { get; init; }
    public required GameTimestamp Timestamp { get; init; }
}

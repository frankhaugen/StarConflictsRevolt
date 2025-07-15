using Frank.PulseFlow;

namespace StarConflictsRevolt.Server.WebApi.Application.Services.Gameplay;

/// <summary>
/// Represents a game tick with explicit tick number and timestamp
/// </summary>
public record GameTickMessage : IPulse
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime Created { get; init; } = DateTime.UtcNow;
    public required GameTickNumber TickNumber { get; init; }
    public required GameTimestamp Timestamp { get; init; }
}
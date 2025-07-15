using StarConflictsRevolt.Clients.Models;

namespace StarConflictsRevolt.Server.WebApi.Application.Services.Gameplay;

/// <summary>
/// Represents a world update delta for client synchronization
/// </summary>
public record WorldUpdateMessage
{
    public required GameSessionId SessionId { get; init; }
    public required WorldDto WorldDelta { get; init; }
    public required GameTimestamp Timestamp { get; init; }
}
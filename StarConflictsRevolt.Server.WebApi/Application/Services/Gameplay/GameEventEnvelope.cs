using StarConflictsRevolt.Server.WebApi.Core.Domain.Events;

namespace StarConflictsRevolt.Server.WebApi.Application.Services.Gameplay;

/// <summary>
/// Represents an event envelope for persistence
/// </summary>
public record GameEventEnvelope
{
    public required GameSessionId SessionId { get; init; }
    public required IGameEvent Event { get; init; }
    public required GameTimestamp Timestamp { get; init; }
    public required GameEventVersion Version { get; init; }
}
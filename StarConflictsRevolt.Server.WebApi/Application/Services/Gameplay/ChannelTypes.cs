using StarConflictsRevolt.Server.WebApi.Core.Domain.Events;
using StarConflictsRevolt.Server.WebApi.Core.Domain.AI;
using StarConflictsRevolt.Clients.Models;
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

/// <summary>
/// Strongly typed tick number to avoid using primitive long
/// </summary>
public record GameTickNumber(long Value)
{
    public static implicit operator long(GameTickNumber tickNumber) => tickNumber.Value;
    public static implicit operator GameTickNumber(long value) => new(value);
    
    public override string ToString() => Value.ToString();
}

/// <summary>
/// Strongly typed game timestamp to avoid using primitive DateTime
/// </summary>
public record GameTimestamp(DateTime Value)
{
    public static implicit operator DateTime(GameTimestamp timestamp) => timestamp.Value;
    public static implicit operator GameTimestamp(DateTime value) => new(value);
    
    public override string ToString() => Value.ToString("O");
}

/// <summary>
/// Represents a game command with session context
/// </summary>
public record GameCommandMessage
{
    public required GameSessionId SessionId { get; init; }
    public required IGameEvent Command { get; init; }
}

/// <summary>
/// Strongly typed session ID to avoid using primitive Guid
/// </summary>
public record GameSessionId(Guid Value)
{
    public static implicit operator Guid(GameSessionId sessionId) => sessionId.Value;
    public static implicit operator GameSessionId(Guid value) => new(value);
    
    public override string ToString() => Value.ToString();
    
    public static GameSessionId New() => new(Guid.NewGuid());
}

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

/// <summary>
/// Strongly typed event version to avoid using primitive int
/// </summary>
public record GameEventVersion(int Value)
{
    public static implicit operator int(GameEventVersion version) => version.Value;
    public static implicit operator GameEventVersion(int value) => new(value);
    
    public override string ToString() => Value.ToString();
}

/// <summary>
/// Represents a world update delta for client synchronization
/// </summary>
public record WorldUpdateMessage
{
    public required GameSessionId SessionId { get; init; }
    public required WorldDto WorldDelta { get; init; }
    public required GameTimestamp Timestamp { get; init; }
}

/// <summary>
/// Represents an AI decision for processing
/// </summary>
public record AiDecisionMessage
{
    public required GameSessionId SessionId { get; init; }
    public required GamePlayerId PlayerId { get; init; }
    public required AiDecision Decision { get; init; }
    public required GameTimestamp Timestamp { get; init; }
}

/// <summary>
/// Strongly typed player ID to avoid using primitive Guid
/// </summary>
public record GamePlayerId(Guid Value)
{
    public static implicit operator Guid(GamePlayerId playerId) => playerId.Value;
    public static implicit operator GamePlayerId(Guid value) => new(value);
    
    public override string ToString() => Value.ToString();
    
    public static GamePlayerId New() => new(Guid.NewGuid());
} 
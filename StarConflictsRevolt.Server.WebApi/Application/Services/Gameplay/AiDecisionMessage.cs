using StarConflictsRevolt.Server.Domain.AI;
using StarConflictsRevolt.Server.Simulation.Engine;

namespace StarConflictsRevolt.Server.WebApi.Application.Services.Gameplay;

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
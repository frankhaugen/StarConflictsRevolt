using StarConflictsRevolt.Server.EventStorage.Abstractions;
using StarConflictsRevolt.Server.Domain.Events;
using StarConflictsRevolt.Server.Simulation.Engine;

namespace StarConflictsRevolt.Server.Application.Services.Gameplay;

/// <summary>
/// Represents a game command with session context
/// </summary>
public record GameCommandMessage
{
    public required GameSessionId SessionId { get; init; }
    public required IGameEvent Command { get; init; }
}
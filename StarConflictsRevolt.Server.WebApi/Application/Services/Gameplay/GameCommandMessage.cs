using StarConflictsRevolt.Server.WebApi.Core.Domain.Events;

namespace StarConflictsRevolt.Server.WebApi.Application.Services.Gameplay;

/// <summary>
/// Represents a game command with session context
/// </summary>
public record GameCommandMessage
{
    public required GameSessionId SessionId { get; init; }
    public required IGameEvent Command { get; init; }
}
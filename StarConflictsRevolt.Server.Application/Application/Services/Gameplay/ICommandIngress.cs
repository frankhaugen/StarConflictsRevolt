using StarConflictsRevolt.Server.EventStorage.Abstractions;
using StarConflictsRevolt.Server.Simulation.Engine;

namespace StarConflictsRevolt.Server.Application.Services.Gameplay;

/// <summary>
/// Accepts commands at the boundary; fast validation only, then enqueue.
/// </summary>
public interface ICommandIngress
{
    ValueTask SubmitAsync(GameSessionId sessionId, IGameCommand command, CancellationToken ct);
}

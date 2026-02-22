using StarConflictsRevolt.Server.EventStorage.Abstractions;

namespace StarConflictsRevolt.Server.Simulation.Engine;

/// <summary>
/// Single-writer (engine) queue for game commands. Commands are drained at tick boundary.
/// </summary>
public interface ICommandQueue
{
    bool TryEnqueue(GameSessionId sessionId, IGameCommand command);
    ValueTask<IReadOnlyList<QueuedCommand>> DrainAsync(CancellationToken ct);
}

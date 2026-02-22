namespace StarConflictsRevolt.Server.Simulation.Engine;

/// <summary>
/// In-process consumer of game ticks. Implemented by the application layer (e.g. game update, AI).
/// Transport calls each registered listener when a tick is published.
/// </summary>
public interface ITickListener
{
    Task OnTickAsync(GameTickMessage tick, CancellationToken cancellationToken);
}

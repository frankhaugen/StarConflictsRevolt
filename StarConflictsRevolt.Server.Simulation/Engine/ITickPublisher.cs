namespace StarConflictsRevolt.Server.Simulation.Engine;

/// <summary>
/// Publishes game ticks to the Transport layer (in-process listeners and SignalR).
/// Consumed by the Ticker; implemented by the host Transport.
/// </summary>
public interface ITickPublisher
{
    Task PublishTickAsync(GameTickMessage tick, CancellationToken cancellationToken);
}

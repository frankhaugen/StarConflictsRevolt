namespace StarConflictsRevolt.Server.Simulation.Engine;

/// <summary>
/// Thread-safe implementation of <see cref="ITickerLiveness"/> for health checks.
/// </summary>
public sealed class TickerLivenessService : ITickerLiveness
{
    private readonly object _lock = new();
    private DateTime? _lastTickUtc;

    public DateTime? LastTickUtc { get { lock (_lock) return _lastTickUtc; } }

    public void SetLastTick(DateTime utc)
    {
        lock (_lock) _lastTickUtc = utc;
    }
}

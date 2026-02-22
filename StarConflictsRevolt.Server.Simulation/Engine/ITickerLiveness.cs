namespace StarConflictsRevolt.Server.Simulation.Engine;

/// <summary>
/// Records the last time the ticker loop ran so health checks can verify ticker liveness.
/// Updated each loop iteration by <see cref="GameTickService"/> (even when paused).
/// </summary>
public interface ITickerLiveness
{
    /// <summary>Last UTC time the ticker loop executed (any iteration).</summary>
    DateTime? LastTickUtc { get; }

    /// <summary>Called by the ticker each loop iteration to record liveness.</summary>
    void SetLastTick(DateTime utc);
}

namespace StarConflictsRevolt.Server.Simulation.Engine;

/// <summary>
/// Controls the simulation ticker (game speed). Can be accessed to read or change ticks per second in real time.
/// The actual tick loop (e.g. <see cref="GameTickService"/>) should use <see cref="GetTickInterval"/> each cycle.
/// </summary>
public interface ISimulationManager
{
    /// <summary>Current ticks per second (1–<see cref="MaxTicksPerSecond"/>).</summary>
    int TicksPerSecond { get; }

    /// <summary>Minimum allowed ticks per second.</summary>
    int MinTicksPerSecond { get; }

    /// <summary>Maximum allowed ticks per second.</summary>
    int MaxTicksPerSecond { get; }

    /// <summary>Interval between ticks based on current <see cref="TicksPerSecond"/>.</summary>
    TimeSpan GetTickInterval();

    /// <summary>Set ticks per second (clamped to <see cref="MinTicksPerSecond"/>–<see cref="MaxTicksPerSecond"/>).</summary>
    void SetTicksPerSecond(int value);

    /// <summary>Whether the simulation is paused (no ticks published).</summary>
    bool IsPaused { get; }

    /// <summary>Pause or resume the simulation.</summary>
    void SetPaused(bool paused);
}

namespace StarConflictsRevolt.Server.Simulation.Engine;

/// <summary>
/// Thread-safe implementation of <see cref="ISimulationManager"/> for use by the ticker and API.
/// </summary>
public sealed class SimulationManager : ISimulationManager
{
    private readonly object _lock = new();
    private int _ticksPerSecond;
    private bool _isPaused;

    public const int DefaultTicksPerSecond = 10;
    private const int MinTps = 1;
    private const int MaxTps = 120;

    public SimulationManager(int initialTicksPerSecond = DefaultTicksPerSecond)
    {
        _ticksPerSecond = Clamp(initialTicksPerSecond, MinTps, MaxTps);
    }

    public int TicksPerSecond
    {
        get { lock (_lock) return _ticksPerSecond; }
    }

    public int MinTicksPerSecond => MinTps;
    public int MaxTicksPerSecond => MaxTps;

    public TimeSpan GetTickInterval()
    {
        lock (_lock)
        {
            var tps = _ticksPerSecond;
            if (tps <= 0) tps = 1;
            return TimeSpan.FromMilliseconds(1000.0 / tps);
        }
    }

    public void SetTicksPerSecond(int value)
    {
        lock (_lock)
            _ticksPerSecond = Clamp(value, MinTps, MaxTps);
    }

    public bool IsPaused
    {
        get { lock (_lock) return _isPaused; }
    }

    public void SetPaused(bool paused)
    {
        lock (_lock) _isPaused = paused;
    }

    private static int Clamp(int value, int min, int max)
    {
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }
}

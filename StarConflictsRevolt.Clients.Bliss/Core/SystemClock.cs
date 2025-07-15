using System.Diagnostics;

namespace StarConflictsRevolt.Clients.Bliss.Core;

/// <summary>
/// System clock implementation using high-resolution timer.
/// </summary>
public class SystemClock : IClock
{
    private readonly Stopwatch _stopwatch;
    private long _lastFrameTicks;
    private float _deltaTime;
    private float _totalTime;

    public long Ticks => _stopwatch.ElapsedTicks;
    public float DeltaTime => _deltaTime;
    public float TotalTime => _totalTime;

    public SystemClock()
    {
        _stopwatch = Stopwatch.StartNew();
        _lastFrameTicks = _stopwatch.ElapsedTicks;
    }

    public void Update()
    {
        var currentTicks = _stopwatch.ElapsedTicks;
        var elapsedTicks = currentTicks - _lastFrameTicks;
        
        _deltaTime = (float)elapsedTicks / Stopwatch.Frequency;
        _totalTime = (float)currentTicks / Stopwatch.Frequency;
        
        _lastFrameTicks = currentTicks;
    }
} 
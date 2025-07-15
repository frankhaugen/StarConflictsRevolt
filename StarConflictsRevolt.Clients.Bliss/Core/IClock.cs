namespace StarConflictsRevolt.Clients.Bliss.Core;

/// <summary>
/// Deterministic tick source for domain logic (removable for tests).
/// </summary>
public interface IClock
{
    /// <summary>
    /// Gets the current time in ticks.
    /// </summary>
    long Ticks { get; }
    
    /// <summary>
    /// Gets the elapsed time since the last frame in seconds.
    /// </summary>
    float DeltaTime { get; }
    
    /// <summary>
    /// Gets the total elapsed time since the clock started in seconds.
    /// </summary>
    float TotalTime { get; }
    
    /// <summary>
    /// Updates the clock for the current frame.
    /// </summary>
    void Update();
} 
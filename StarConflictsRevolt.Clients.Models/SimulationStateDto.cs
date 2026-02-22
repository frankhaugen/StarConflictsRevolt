namespace StarConflictsRevolt.Clients.Models;

/// <summary>Current simulation (ticker) state from the server.</summary>
public class SimulationStateDto
{
    public int TicksPerSecond { get; set; }
    public int MinTicksPerSecond { get; set; }
    public int MaxTicksPerSecond { get; set; }
    public bool IsPaused { get; set; }
}

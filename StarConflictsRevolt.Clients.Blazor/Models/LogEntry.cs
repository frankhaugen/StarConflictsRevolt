namespace StarConflictsRevolt.Clients.Blazor.Models;

/// <summary>
/// Represents a log entry for the diagnostics page
/// </summary>
public class LogEntry
{
    public DateTime Timestamp { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

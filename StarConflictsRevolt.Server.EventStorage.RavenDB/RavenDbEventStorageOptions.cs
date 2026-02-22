namespace StarConflictsRevolt.Server.EventStorage.RavenDB;

/// <summary>
/// Configuration for RavenDB event storage.
/// </summary>
public sealed class RavenDbEventStorageOptions
{
    /// <summary>RavenDB server URL (e.g. http://localhost:8090).</summary>
    public string Url { get; set; } = "http://localhost:8090";

    /// <summary>Database name.</summary>
    public string DatabaseName { get; set; } = "StarConflictsRevolt";

    /// <summary>Channel capacity for the publish loop.</summary>
    public int ChannelCapacity { get; set; } = 1000;
}

namespace StarConflictsRevolt.Server.Storage.JsonFiles;

/// <summary>
/// Configuration for the JSON directory storage provider.
/// Layout: RootPath/&lt;TypeName&gt;/&lt;Id:N&gt;.json (Id = 32 hex chars, no dashes).
/// Optional process lock: RootPath/.store.lock
/// </summary>
public sealed class JsonFilesOptions
{
    /// <summary>Root directory for all entity type folders.</summary>
    public string RootPath { get; set; } = string.Empty;

    /// <summary>If true, no writes or deletes; startup skips write/atomic-replace checks.</summary>
    public bool ReadOnly { get; set; }

    /// <summary>If true and not ReadOnly, root directory is created on registration when missing.</summary>
    public bool CreateIfMissing { get; set; } = true;

    /// <summary>If true and not ReadOnly, acquire process-wide lock file (RootPath/.store.lock) on startup.</summary>
    public bool UseProcessLock { get; set; }

    /// <summary>Number of stripes for write/delete concurrency (keyed by type + id).</summary>
    public int LockStripes { get; set; } = 64;
}

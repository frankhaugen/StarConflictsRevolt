namespace StarConflictsRevolt.Server.Storage.JsonFiles;

internal static class JsonFilesStartupValidator
{
    /// <summary>
    /// Fail-fast: ensure root exists or can be created, verify enumeration and (if not read-only) write/delete and atomic replace.
    /// </summary>
    public static void Validate(JsonFilesOptions options, out IDisposable? processLock)
    {
        processLock = null;
        var root = Path.GetFullPath(options.RootPath ?? string.Empty);
        if (string.IsNullOrWhiteSpace(root))
            throw new InvalidOperationException("JsonFilesOptions.RootPath must be set.");

        if (!Directory.Exists(root))
        {
            if (options.ReadOnly || !options.CreateIfMissing)
                throw new InvalidOperationException($"Root path does not exist and cannot be created: {root}");
            Directory.CreateDirectory(root);
        }

        // Verify directory enumeration
        _ = Directory.EnumerateFileSystemEntries(root).Take(1).ToList();

        if (!options.ReadOnly)
        {
            // Probe write/read/delete in root
            var probe = Path.Combine(root, ".probe_" + Guid.NewGuid().ToString("N"));
            try
            {
                File.WriteAllText(probe, "x");
                var read = File.ReadAllText(probe);
                if (read != "x")
                    throw new InvalidOperationException($"Read-back failed in root: {root}");
            }
            finally
            {
                if (File.Exists(probe))
                    File.Delete(probe);
            }

            // Verify atomic replace (File.Move(tmp, final, overwrite: true))
            var final = Path.Combine(root, ".atomic_" + Guid.NewGuid().ToString("N"));
            var tmp = final + ".tmp";
            try
            {
                File.WriteAllText(tmp, "a");
                var fs = new FileStream(tmp, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                fs.Flush(flushToDisk: true);
                fs.Dispose();
                File.Move(tmp, final, overwrite: true);
                if (!File.Exists(final) || File.ReadAllText(final) != "a")
                    throw new InvalidOperationException($"Atomic replace check failed in root: {root}");
            }
            finally
            {
                if (File.Exists(tmp)) File.Delete(tmp);
                if (File.Exists(final)) File.Delete(final);
            }
        }

        if (!options.ReadOnly && options.UseProcessLock)
        {
            var lockPath = Path.Combine(root, ".store.lock");
            var stream = new FileStream(lockPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, 1, FileOptions.None);
            processLock = new ProcessLockHolder(stream);
        }
    }

    private sealed class ProcessLockHolder(FileStream stream) : IDisposable
    {
        public void Dispose() => stream.Dispose();
    }
}

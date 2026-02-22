using System.Collections.Concurrent;
using System.Text.Json;
using StarConflictsRevolt.Server.Storage.Abstractions;

namespace StarConflictsRevolt.Server.Storage.JsonFiles;

internal sealed class JsonFilesRepository<T> : IRepository<T> where T : class, IHasId
{
    private readonly string _typeDir;
    private readonly JsonFilesOptions _options;
    private readonly SemaphoreSlim[] _stripes;
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = false
    };

    public JsonFilesRepository(JsonFilesOptions options)
    {
        _options = options;
        _typeDir = Path.Combine(_options.RootPath, typeof(T).Name);
        _stripes = new SemaphoreSlim[Math.Max(1, _options.LockStripes)];
        for (var i = 0; i < _stripes.Length; i++)
            _stripes[i] = new SemaphoreSlim(1, 1);
    }

    private SemaphoreSlim GetStripe(Guid id)
    {
        var idx = Math.Abs(unchecked((typeof(T).FullName?.GetHashCode(StringComparison.Ordinal) ?? 0) + id.GetHashCode())) % _stripes.Length;
        return _stripes[idx];
    }

    private static string IdToFileName(Guid id) => id.ToString("N") + ".json";

    private string GetFilePath(Guid id) => Path.Combine(_typeDir, IdToFileName(id));

    public IEnumerable<T> All()
    {
        if (!Directory.Exists(_typeDir))
            yield break;

        foreach (var file in Directory.EnumerateFiles(_typeDir, "*.json"))
        {
            var entity = TryReadFile(file);
            if (entity != null)
                yield return entity;
        }
    }

    private static T? TryReadFile(string path)
    {
        try
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<T>(json, SerializerOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public async ValueTask<T?> TryGetAsync(Guid id, CancellationToken ct = default)
    {
        var path = GetFilePath(id);
        if (!File.Exists(path))
            return null;

        try
        {
            await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous);
            var entity = await JsonSerializer.DeserializeAsync<T>(stream, SerializerOptions, ct).ConfigureAwait(false);
            return entity;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public async ValueTask UpsertAsync(T entity, CancellationToken ct = default)
    {
        if (entity.Id == Guid.Empty)
            throw new ArgumentException("Entity Id must not be empty.", nameof(entity));

        if (_options.ReadOnly)
            throw new InvalidOperationException("JSON files store is configured read-only.");

        var id = entity.Id;
        var sem = GetStripe(id);
        await sem.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            var path = GetFilePath(id);
            var dir = Path.GetDirectoryName(path)!;
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var tmp = path + ".tmp";
            await using (var stream = new FileStream(tmp, FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.Asynchronous))
            {
                await JsonSerializer.SerializeAsync(stream, entity, SerializerOptions, ct).ConfigureAwait(false);
                await stream.FlushAsync(ct).ConfigureAwait(false);
                stream.Flush(flushToDisk: true);
            }

            File.Move(tmp, path, overwrite: true);
        }
        finally
        {
            sem.Release();
        }
    }

    public async ValueTask<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        if (_options.ReadOnly)
            throw new InvalidOperationException("JSON files store is configured read-only.");

        var sem = GetStripe(id);
        await sem.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            var path = GetFilePath(id);
            if (!File.Exists(path))
                return false;
            File.Delete(path);
            return true;
        }
        finally
        {
            sem.Release();
        }
    }
}

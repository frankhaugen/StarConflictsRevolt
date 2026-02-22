using Microsoft.Extensions.DependencyInjection;
using StarConflictsRevolt.Server.Storage.Abstractions;

namespace StarConflictsRevolt.Server.Storage.JsonFiles;

/// <summary>
/// Registers the JSON directory provider: one folder per entity type, one file per entity (RootPath/TypeName/IdN.json).
/// Uses System.Text.Json; atomic replace on write; striped locks for writes/deletes; optional process lock file.
/// </summary>
public static class JsonFilesStorageExtensions
{
    /// <summary>
    /// Adds the JSON files provider. Runs fail-fast startup validation (root exists or created, enumeration, and if not read-only: write/delete and atomic replace; optionally acquires .store.lock).
    /// </summary>
    public static IStorageBuilder AddJsonFilesProvider(
        this IStorageBuilder builder,
        Action<JsonFilesOptions> configure)
    {
        var options = new JsonFilesOptions();
        configure(options);

        JsonFilesStartupValidator.Validate(options, out var processLock);

        builder.Services.AddSingleton(options);
        if (processLock != null)
            builder.Services.AddSingleton(processLock);
        builder.Services.AddSingleton<IRepositoryProvider, JsonFilesRepositoryProvider>();
        builder.Services.AddTransient(typeof(IRepository<>), typeof(JsonFilesRepository<>));

        return builder;
    }
}

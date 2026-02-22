using LiteDB;
using Microsoft.Extensions.DependencyInjection;
using StarConflictsRevolt.Server.Storage.Abstractions;

namespace StarConflictsRevolt.Server.Storage.LiteDb;

/// <summary>
/// Registers LiteDB as the default repository backend and exposes <see cref="ILiteDatabase"/> for legacy or custom usage.
/// </summary>
public static class LiteDbStorageExtensions
{
    /// <summary>
    /// Adds the LiteDB provider: options, <see cref="ILiteDatabase"/> singleton, <see cref="IRepositoryProvider"/>, and open generic <see cref="IRepository{T}"/>.
    /// </summary>
    public static IStorageBuilder AddLiteDbProvider(
        this IStorageBuilder builder,
        Action<LiteDbOptions> configure)
    {
        var opt = new LiteDbOptions();
        configure(opt);

        var connectionString = BuildConnectionString(opt);
        var db = new LiteDatabase(connectionString);

        builder.Services.AddSingleton(opt);
        builder.Services.AddSingleton<ILiteDatabase>(db);
        builder.Services.AddSingleton<IRepositoryProvider, LiteDbRepositoryProvider>();
        builder.Services.AddTransient(typeof(IRepository<>), typeof(LiteDbRepository<>));

        return builder;
    }

    private static string BuildConnectionString(LiteDbOptions opt)
    {
        var path = opt.DatabasePath ?? string.Empty;
        if (!path.Contains('='))
            path = "Filename=" + path;
        if (!string.IsNullOrEmpty(opt.Password))
            path += ";Password=" + opt.Password;
        return path;
    }
}

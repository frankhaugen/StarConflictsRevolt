using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Raven.Client.Documents;
using Raven.Client.Documents.Operations;
using Raven.Client.Exceptions.Database;
using Raven.Client.ServerWide;
using Raven.Client.ServerWide.Operations;
using StarConflictsRevolt.Server.EventStorage.Abstractions;

namespace StarConflictsRevolt.Server.EventStorage.RavenDB;

/// <summary>
/// Registers RavenDB as the event store backend: document store, database creation, and <see cref="IEventStore"/> → <see cref="RavenEventStore"/>.
/// </summary>
public static class RavenDbEventStorageExtensions
{
    /// <summary>
    /// Adds RavenDB event storage: resolves URL and database name from <paramref name="configuration"/> (ConnectionStrings:ravenDb or default),
    /// creates and initializes the document store, ensures the database exists, then registers <see cref="IDocumentStore"/> and <see cref="IEventStore"/>.
    /// </summary>
    public static IServiceCollection AddRavenDbEventStorage(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("ravenDb");
        string url;
        if (connectionString?.StartsWith("URL=", StringComparison.OrdinalIgnoreCase) == true)
            url = connectionString.Substring(4).Trim();
        else
            url = connectionString ?? "http://localhost:8090";

        const string databaseName = "StarConflictsRevolt";
        var documentStore = new DocumentStore
        {
            Urls = new[] { url },
            Database = databaseName
        }.Initialize();

        EnsureRavenDatabaseExists(documentStore, databaseName);

        services.AddSingleton(documentStore);
        services.AddSingleton<IEventStore>(sp =>
        {
            var store = sp.GetRequiredService<IDocumentStore>();
            var logger = sp.GetRequiredService<ILogger<RavenEventStore>>();
            return new RavenEventStore(store, logger, capacity: 1000);
        });

        return services;
    }

    /// <summary>
    /// Adds RavenDB event storage with explicit options.
    /// </summary>
    public static IServiceCollection AddRavenDbEventStorage(this IServiceCollection services, Action<RavenDbEventStorageOptions> configure)
    {
        var options = new RavenDbEventStorageOptions();
        configure(options);

        var documentStore = new DocumentStore
        {
            Urls = new[] { options.Url },
            Database = options.DatabaseName
        }.Initialize();

        EnsureRavenDatabaseExists(documentStore, options.DatabaseName);

        services.AddSingleton<IDocumentStore>(documentStore);
        services.AddSingleton<IEventStore>(sp =>
        {
            var store = sp.GetRequiredService<IDocumentStore>();
            var logger = sp.GetRequiredService<ILogger<RavenEventStore>>();
            return new RavenEventStore(store, logger, options.ChannelCapacity);
        });

        return services;
    }

    /// <summary>
    /// Creates the RavenDB database if it does not exist (e.g. fresh container or first run).
    /// </summary>
    public static void EnsureRavenDatabaseExists(IDocumentStore store, string databaseName)
    {
        try
        {
            store.Maintenance.ForDatabase(databaseName).Send(new GetStatisticsOperation());
            return;
        }
        catch (DatabaseDoesNotExistException)
        {
            try
            {
                store.Maintenance.Server.Send(new CreateDatabaseOperation(new DatabaseRecord(databaseName)));
                Console.WriteLine("RavenDB database '{0}' created.", databaseName);
            }
            catch (Exception ex)
            {
                Console.WriteLine("RavenDB database creation skipped or failed: {0}", ex.Message);
            }
        }
    }
}

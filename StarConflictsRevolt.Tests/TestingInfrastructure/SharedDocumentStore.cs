using Raven.Client.Documents;

namespace StarConflictsRevolt.Tests.TestingInfrastructure;

internal static class SharedDocumentStore
{
    private static readonly RavenTUnitDriver Driver = new();
    private static readonly Dictionary<string, IDocumentStore> Stores = new();
    
    public static IDocumentStore CreateStore(string database)
    {
        if (Stores.TryGetValue(database, out var store))
        {
            return store;
        }

        store = Driver.NewStore(database);
        Stores[database] = store;
        return store;
    }
} 
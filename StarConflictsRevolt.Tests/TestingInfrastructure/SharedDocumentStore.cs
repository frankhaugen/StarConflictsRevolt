using Raven.Client.Documents;

namespace StarConflictsRevolt.Tests.TestingInfrastructure;

internal static class SharedDocumentStore
{
    private static readonly RavenTUnitDriver Driver = new();
    private static readonly Dictionary<string, IDocumentStore> Stores = new();

    public static IDocumentStore CreateStore(string database)
    {
        var threadId = Environment.CurrentManagedThreadId;

        var storeKey = $"{database}-{threadId}";

        if (Stores.TryGetValue(storeKey, out var store)) return store;

        store = Driver.NewStore(storeKey);
        Stores[storeKey] = store;
        return store;
    }
}
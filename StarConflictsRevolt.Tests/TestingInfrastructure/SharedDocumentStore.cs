using System.Collections.Concurrent;
using Raven.Client.Documents;

namespace StarConflictsRevolt.Tests.TestingInfrastructure;

internal static class SharedDocumentStore
{
    private static readonly RavenTUnitDriver Driver = new();
    private static readonly ConcurrentDictionary<string, IDocumentStore> Stores = new();

    public static IDocumentStore CreateStore(string database)
    {
        var threadId = Environment.CurrentManagedThreadId;

        var storeKey = $"{database}-{threadId}";

        return Stores.GetOrAdd(storeKey, key => Driver.NewStore(key));
    }
}
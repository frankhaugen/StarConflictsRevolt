using Raven.Client.Documents;

namespace StarConflictsRevolt.Tests.TestingInfrastructure;

internal static class SharedDocumentStore
{
    private static readonly RavenTUnitDriver Driver = new();
    private static readonly Lazy<IDocumentStore> _store = new(() => Driver.NewStore("Shared"));
    
    public static IDocumentStore Instance => _store.Value;
} 
using Raven.Client.Documents;
using Raven.TestDriver;

namespace StarConflictsRevolt.Tests.TestingInfrastructure;

internal sealed class RavenTUnitDriver : RavenTestDriver
{
    public IDocumentStore NewStore(string caller)
    {
        var store = GetDocumentStore();
        store.Database = $"{caller}_{Guid.NewGuid():N}";
        return store;
    }
} 
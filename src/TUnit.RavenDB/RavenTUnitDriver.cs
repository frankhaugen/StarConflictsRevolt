file-scoped namespace TUnit.RavenDB;

using Raven.Client.Documents;
using Raven.TestDriver;

internal sealed class RavenTUnitDriver : RavenTestDriver
{
    public IDocumentStore NewStore(string caller) =>
        GetDocumentStore(new() { Database = $"{caller}_{Guid.NewGuid():N}" });
} 
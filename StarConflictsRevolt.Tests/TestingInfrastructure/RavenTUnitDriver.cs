using Raven.Client.Documents;
using Raven.TestDriver;

namespace StarConflictsRevolt.Tests.TestingInfrastructure;

internal sealed class RavenTUnitDriver : RavenTestDriver
{
    public IDocumentStore NewStore(string database) =>
        GetDocumentStore(database: database);
} 
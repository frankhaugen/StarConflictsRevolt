using Raven.Client.Documents;

namespace StarConflictsRevolt.Tests.TestingInfrastructure;

public interface IDocumentStoreProvider
{
    IDocumentStore GetStore(string? dbName = null);
} 
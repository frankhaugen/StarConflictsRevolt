using Raven.Client.Documents.Session;
using TUnit.Assertions.Extensions;
using TUnit.Core;
using StarConflictsRevolt.Tests.TestingInfrastructure;

namespace StarConflictsRevolt.Tests.ServerTests.IntegrationTests;

[RavenDbDataSource]
public partial class SimplePersistenceTests(IAsyncDocumentSession session)
{
    [Test]
    public async Task Can_roundtrip_entity()
    {
        var e = new Foo { Name = "Bar" };

        await session.StoreAsync(e);
        await session.SaveChangesAsync();

        await Assert.That(await session.LoadAsync<Foo>(e.Id)).IsNotNull();
    }

    private sealed class Foo
    {
        public string Id { get; set; } = default!;
        public string Name { get; init; } = string.Empty;
    }
} 
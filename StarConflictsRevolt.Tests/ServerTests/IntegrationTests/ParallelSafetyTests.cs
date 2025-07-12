using Raven.Client.Documents.Session;
using TUnit.Core;
using StarConflictsRevolt.Tests.TestingInfrastructure;

namespace StarConflictsRevolt.Tests.ServerTests.IntegrationTests;

[RavenDbDataSource]
public partial class ParallelSafetyTests(IAsyncDocumentSession session)
{
    [Test]
    public async Task Concurrent_sessions_do_not_clash()
    {
        var range = Enumerable.Range(0, 1000);
        await Parallel.ForEachAsync(range, async (i, _) =>
        {
            var entity = new Item { Value = i };
            await session.StoreAsync(entity);
            await session.SaveChangesAsync();

            var loaded = await session.LoadAsync<Item>(entity.Id);
            if (loaded?.Value != i)
                throw new InvalidOperationException("Data corruption!");
        });
    }

    private sealed class Item
    {
        public string Id { get; set; } = default!;
        public int Value { get; init; }
    }
} 
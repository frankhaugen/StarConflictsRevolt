using StarConflictsRevolt.Tests.TestingInfrastructure;

namespace StarConflictsRevolt.Tests.ServerTests.IntegrationTests;

public partial class ParallelSafetyTests
{
    [Test]
    [Timeout(20_000)]
    public async Task Concurrent_sessions_do_not_clash(CancellationToken cancellationToken)
    {
        var store = SharedDocumentStore.CreateStore("ParallelSafetyTests" + Random.Shared.Next(10,1000));
        var range = Enumerable.Range(0, 1000);
        await Parallel.ForEachAsync(range, cancellationToken, async (i, ct) =>
        {
            // Create a new session for each operation to avoid disposal issues
            using var session = store.OpenAsyncSession();
            var entity = new Item { Value = i };
            await session.StoreAsync(entity, ct);
            await session.SaveChangesAsync(ct);

            var loaded = await session.LoadAsync<Item>(entity.Id, ct);
            if (loaded?.Value != i)
                throw new InvalidOperationException("Data corruption!");
            
            // Simulate some processing
            await Task.Delay(1, ct);
        });
    }

    private sealed class Item
    {
        public string Id { get; set; } = default!;
        public int Value { get; init; }
    }
} 
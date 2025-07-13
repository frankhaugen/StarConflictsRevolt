namespace StarConflictsRevolt.Tests.TestingInfrastructure.InfrastructureTests;

public class ParallelSafetyTests
{
    [Test]
    public async Task Concurrent_sessions_do_not_clash()
    {
        var range = Enumerable.Range(0, 1000);

        await Parallel.ForEachAsync(range, async (i, ct) =>
        {
            using var session = SharedDocumentStore.CreateStore("ParallelSafetyTests").OpenAsyncSession();
            var entity = new Item { Value = i };
            await session.StoreAsync(entity, ct);
            await session.SaveChangesAsync(ct);

            var loaded = await session.LoadAsync<Item>(entity.Id, ct);
            if (loaded?.Value != i)
                throw new InvalidOperationException("Data corruption!");
        });
    }

    private sealed class Item
    {
        public string Id { get; } = default!;
        public int Value { get; init; }
    }
}
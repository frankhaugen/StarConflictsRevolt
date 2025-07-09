using StarConflictsRevolt.Server.Eventing;

namespace StarConflictsRevolt.Tests.ServerTests;

public class InMemoryEventStore : IEventStore
{
    public Task PublishAsync(Guid worldId, IGameEvent @event) => Task.CompletedTask;
    public Task SubscribeAsync(Func<EventEnvelope, Task> handler, CancellationToken ct) => Task.CompletedTask;
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
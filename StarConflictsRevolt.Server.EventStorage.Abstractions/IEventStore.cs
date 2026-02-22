namespace StarConflictsRevolt.Server.EventStorage.Abstractions;

/// <summary>
/// Event store contract: publish events and subscribe to persisted events.
/// </summary>
public interface IEventStore : IAsyncDisposable
{
    Task PublishAsync(Guid worldId, IGameEvent @event);
    Task SubscribeAsync(Func<EventEnvelope, Task> handler, CancellationToken ct);
}

namespace StarConflictsRevolt.Server.WebApi.Eventing;

public interface IEventStore : IAsyncDisposable
{
    Task PublishAsync(Guid worldId, IGameEvent @event);
    Task SubscribeAsync(Func<EventEnvelope, Task> handler, CancellationToken ct);
}
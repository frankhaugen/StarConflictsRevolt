using StarConflictsRevolt.Server.WebApi.Eventing;

namespace StarConflictsRevolt.Tests.TestingInfrastructure;

public class InMemoryEventStore : IEventStore
{
    private readonly Dictionary<Guid, List<EventEnvelope>> _events = new();
    
    public Task PublishAsync(Guid worldId, IGameEvent @event) => 
        Task.Run(() =>
        {
            if (!_events.ContainsKey(worldId))
            {
                _events[worldId] = new List<EventEnvelope>();
            }
            _events[worldId].Add(new EventEnvelope(@worldId, @event, DateTime.UtcNow));
        });
    
    public Task SubscribeAsync(Func<EventEnvelope, Task> handler, CancellationToken ct) => 
        Task.Run(() =>
        {
            // In-memory event store does not support subscriptions.
            // This is a no-op for testing purposes.
        }, ct);
    
    public ValueTask DisposeAsync() => 
        ValueTask.CompletedTask;
}
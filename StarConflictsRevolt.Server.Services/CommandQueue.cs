using System.Collections.Concurrent;

namespace StarConflictsRevolt.Server.Core;

public class CommandQueue<TCommand>
{
    private readonly ConcurrentDictionary<Guid, ConcurrentQueue<TCommand>> _queues = new();

    public void Enqueue(Guid sessionId, TCommand command)
    {
        var queue = _queues.GetOrAdd(sessionId, _ => new ConcurrentQueue<TCommand>());
        queue.Enqueue(command);
    }

    public bool TryDequeue(Guid sessionId, out TCommand command)
    {
        command = default!;
        if (_queues.TryGetValue(sessionId, out var queue))
            return queue.TryDequeue(out command);
        return false;
    }

    public ConcurrentQueue<TCommand> GetOrCreateQueue(Guid sessionId)
        => _queues.GetOrAdd(sessionId, _ => new ConcurrentQueue<TCommand>());
} 
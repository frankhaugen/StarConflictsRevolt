using System.Collections.Concurrent;

namespace StarConflictsRevolt.Server.WebApi.Application.Services.Gameplay;

public class CommandQueue<TCommand>
{
    private readonly ILogger<CommandQueue<TCommand>> _logger;
    private readonly ConcurrentDictionary<Guid, ConcurrentQueue<TCommand>> _queues = new();

    public CommandQueue(ILogger<CommandQueue<TCommand>> logger)
    {
        _logger = logger;
    }

    public void Enqueue(Guid sessionId, TCommand command)
    {
        var queue = _queues.GetOrAdd(sessionId, _ => new ConcurrentQueue<TCommand>());
        queue.Enqueue(command);
        _logger.LogInformation("Enqueued command {CommandType} for session {SessionId}", command?.GetType().Name, sessionId);
    }

    public bool TryDequeue(Guid sessionId, out TCommand command)
    {
        command = default!;
        if (_queues.TryGetValue(sessionId, out var queue))
        {
            var dequeued = queue.TryDequeue(out command);
            if (dequeued) _logger.LogInformation("Dequeued command {CommandType} for session {SessionId}", command?.GetType().Name, sessionId);
            return dequeued;
        }

        return false;
    }

    public ConcurrentQueue<TCommand> GetOrCreateQueue(Guid sessionId)
    {
        return _queues.GetOrAdd(sessionId, _ => new ConcurrentQueue<TCommand>());
    }
}
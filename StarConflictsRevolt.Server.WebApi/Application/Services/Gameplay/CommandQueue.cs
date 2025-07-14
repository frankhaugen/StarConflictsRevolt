using System.Collections.Concurrent;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Events;

namespace StarConflictsRevolt.Server.WebApi.Application.Services.Gameplay;

public class CommandQueue
{
    private readonly ILogger<CommandQueue> _logger;
    private readonly ConcurrentDictionary<GameSessionId, ConcurrentQueue<GameCommandMessage>> _queues = new();

    public CommandQueue(ILogger<CommandQueue> logger)
    {
        _logger = logger;
    }

    public void Enqueue(GameSessionId sessionId, IGameEvent command)
    {
        var commandMessage = new GameCommandMessage
        {
            SessionId = sessionId,
            Command = command
        };
        
        var queue = _queues.GetOrAdd(sessionId, _ => new ConcurrentQueue<GameCommandMessage>());
        queue.Enqueue(commandMessage);
        _logger.LogInformation("Enqueued command {CommandType} for session {SessionId}", command?.GetType().Name, sessionId);
    }

    public void Enqueue(Guid sessionId, IGameEvent command)
    {
        Enqueue(new GameSessionId(sessionId), command);
    }

    public bool TryDequeue(GameSessionId sessionId, out GameCommandMessage commandMessage)
    {
        commandMessage = default!;
        if (_queues.TryGetValue(sessionId, out var queue))
        {
            var dequeued = queue.TryDequeue(out commandMessage);
            if (dequeued) _logger.LogInformation("Dequeued command {CommandType} for session {SessionId}", commandMessage.Command?.GetType().Name, sessionId);
            return dequeued;
        }

        return false;
    }

    public bool TryDequeue(Guid sessionId, out GameCommandMessage commandMessage)
    {
        return TryDequeue(new GameSessionId(sessionId), out commandMessage);
    }

    public ConcurrentQueue<GameCommandMessage> GetOrCreateQueue(GameSessionId sessionId)
    {
        return _queues.GetOrAdd(sessionId, _ => new ConcurrentQueue<GameCommandMessage>());
    }

    public int GetQueueCount(GameSessionId sessionId)
    {
        return _queues.TryGetValue(sessionId, out var queue) ? queue.Count : 0;
    }

    public int GetQueueCount(Guid sessionId)
    {
        return GetQueueCount(new GameSessionId(sessionId));
    }

    public bool HasCommands(GameSessionId sessionId)
    {
        return GetQueueCount(sessionId) > 0;
    }

    public bool HasCommands(Guid sessionId)
    {
        return HasCommands(new GameSessionId(sessionId));
    }
}
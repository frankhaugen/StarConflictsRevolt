using System.Threading.Channels;
using StarConflictsRevolt.Server.EventStorage.Abstractions;
using StarConflictsRevolt.Server.Simulation.Engine;

namespace StarConflictsRevolt.Server.Application.Services.Gameplay;

/// <summary>
/// Channel-based command queue; single reader (engine), multiple writers (ingress/hub).
/// </summary>
public sealed class CommandQueueChannel : ICommandQueue
{
    private readonly Channel<QueuedCommand> _channel = Channel.CreateUnbounded<QueuedCommand>(
        new UnboundedChannelOptions { SingleReader = true, SingleWriter = false });
    private readonly ILogger<CommandQueueChannel> _logger;

    public CommandQueueChannel(ILogger<CommandQueueChannel> logger)
    {
        _logger = logger;
    }

    public bool TryEnqueue(GameSessionId sessionId, IGameCommand command)
    {
        var queued = new QueuedCommand(sessionId, command);
        if (!_channel.Writer.TryWrite(queued))
        {
            _logger.LogWarning("Command queue write failed for session {SessionId}", sessionId);
            return false;
        }
        _logger.LogDebug("Enqueued command {CommandType} for session {SessionId}", command.GetType().Name, sessionId);
        return true;
    }

    public async ValueTask<IReadOnlyList<QueuedCommand>> DrainAsync(CancellationToken ct)
    {
        var list = new List<QueuedCommand>(64);
        while (_channel.Reader.TryRead(out var queued))
            list.Add(queued);
        await ValueTask.CompletedTask;
        return list;
    }
}

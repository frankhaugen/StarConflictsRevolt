using StarConflictsRevolt.Server.EventStorage.Abstractions;
using StarConflictsRevolt.Server.Simulation.Engine;

namespace StarConflictsRevolt.Server.Application.Services.Gameplay;

public sealed class CommandIngress : ICommandIngress
{
    private readonly ICommandQueue _queue;

    public CommandIngress(ICommandQueue queue)
    {
        _queue = queue;
    }

    public ValueTask SubmitAsync(GameSessionId sessionId, IGameCommand command, CancellationToken ct)
    {
        if (!_queue.TryEnqueue(sessionId, command))
            throw new InvalidOperationException("Command queue full");
        return ValueTask.CompletedTask;
    }
}

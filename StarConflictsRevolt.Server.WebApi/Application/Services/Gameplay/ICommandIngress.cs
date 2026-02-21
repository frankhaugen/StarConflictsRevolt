using StarConflictsRevolt.Server.WebApi.Core.Domain.Commands;

namespace StarConflictsRevolt.Server.WebApi.Application.Services.Gameplay;

/// <summary>
/// Accepts commands at the boundary; fast validation only, then enqueue.
/// </summary>
public interface ICommandIngress
{
    ValueTask SubmitAsync(GameSessionId sessionId, IGameCommand command, CancellationToken ct);
}

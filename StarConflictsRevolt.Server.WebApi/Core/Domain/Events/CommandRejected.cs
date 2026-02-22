using StarConflictsRevolt.Server.EventStorage.Abstractions;

namespace StarConflictsRevolt.Server.WebApi.Core.Domain.Events;

/// <summary>
/// Fact: a command was rejected (e.g. validation failed). No world state change.
/// </summary>
public sealed record CommandRejected(long Tick, Guid PlayerId, string Reason) : IGameEvent
{
    public void ApplyTo(object world, ILogger logger)
    {
        logger.LogWarning("Command rejected for player {PlayerId}: {Reason}", PlayerId, Reason);
    }
}

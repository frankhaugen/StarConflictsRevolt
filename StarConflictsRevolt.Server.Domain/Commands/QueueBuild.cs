using StarConflictsRevolt.Server.EventStorage.Abstractions;

namespace StarConflictsRevolt.Server.Domain.Commands;

public sealed record QueueBuild(Guid PlayerId, long ClientTick, Guid AtSystemId, string Design, int Count) : IGameCommand;

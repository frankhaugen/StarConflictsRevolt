using StarConflictsRevolt.Server.EventStorage.Abstractions;

namespace StarConflictsRevolt.Server.Domain.Commands;

public sealed record StartRally(Guid PlayerId, long ClientTick, Guid RegionId) : IGameCommand;

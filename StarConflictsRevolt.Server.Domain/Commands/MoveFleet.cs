using StarConflictsRevolt.Server.EventStorage.Abstractions;

namespace StarConflictsRevolt.Server.Domain.Commands;

public sealed record MoveFleet(Guid PlayerId, long ClientTick, Guid FleetId, Guid ToSystemId) : IGameCommand;

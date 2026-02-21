namespace StarConflictsRevolt.Server.WebApi.Core.Domain.Commands;

public sealed record MoveFleet(Guid PlayerId, long ClientTick, Guid FleetId, Guid ToSystemId) : IGameCommand;

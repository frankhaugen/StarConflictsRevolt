namespace StarConflictsRevolt.Server.WebApi.Core.Domain.Commands;

public sealed record StartRally(Guid PlayerId, long ClientTick, Guid RegionId) : IGameCommand;

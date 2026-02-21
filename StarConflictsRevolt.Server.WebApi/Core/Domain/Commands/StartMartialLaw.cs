namespace StarConflictsRevolt.Server.WebApi.Core.Domain.Commands;

public sealed record StartMartialLaw(Guid PlayerId, long ClientTick, Guid SystemId) : IGameCommand;

namespace StarConflictsRevolt.Server.WebApi.Core.Domain.Commands;

public sealed record QueueBuild(Guid PlayerId, long ClientTick, Guid AtSystemId, string Design, int Count) : IGameCommand;

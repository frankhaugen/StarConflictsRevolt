using StarConflictsRevolt.Server.EventStorage.Abstractions;

namespace StarConflictsRevolt.Server.Domain.Commands;

public sealed record StartMartialLaw(Guid PlayerId, long ClientTick, Guid SystemId) : IGameCommand;

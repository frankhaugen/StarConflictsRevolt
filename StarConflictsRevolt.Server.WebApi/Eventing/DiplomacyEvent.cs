namespace StarConflictsRevolt.Server.WebApi.Eventing;

public record DiplomacyEvent(Guid PlayerId, Guid TargetPlayerId, string ProposalType, string? Message) : IGameEvent;
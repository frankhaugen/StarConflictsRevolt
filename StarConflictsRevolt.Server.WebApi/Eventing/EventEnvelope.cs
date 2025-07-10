namespace StarConflictsRevolt.Server.WebApi.Eventing;

public record EventEnvelope(Guid WorldId, IGameEvent Event, DateTime Timestamp);
namespace StarConflictsRevolt.Server.WebApi.Core.Domain.Events;

public record EventEnvelope(Guid WorldId, IGameEvent Event, DateTime Timestamp);
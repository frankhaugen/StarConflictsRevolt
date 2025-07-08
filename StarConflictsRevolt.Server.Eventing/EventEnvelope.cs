using Raven.Client.Documents.Session;

namespace StarConflictsRevolt.Server.Eventing;

public record EventEnvelope(Guid WorldId, IGameEvent Event, DateTime Timestamp);
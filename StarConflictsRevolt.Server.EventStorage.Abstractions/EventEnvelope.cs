namespace StarConflictsRevolt.Server.EventStorage.Abstractions;

/// <summary>
/// Wrapper for a game event with world id and timestamp.
/// </summary>
public record EventEnvelope(Guid WorldId, IGameEvent Event, DateTime Timestamp);

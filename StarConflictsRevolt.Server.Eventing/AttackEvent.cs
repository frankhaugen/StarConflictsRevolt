namespace StarConflictsRevolt.Server.Eventing;

public record AttackEvent(Guid PlayerId, Guid AttackerFleetId, Guid DefenderFleetId, Guid LocationPlanetId) : IGameEvent;
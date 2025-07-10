namespace StarConflictsRevolt.Server.WebApi.Eventing;

public record AttackEvent(Guid PlayerId, Guid AttackerFleetId, Guid DefenderFleetId, Guid LocationPlanetId) : IGameEvent;
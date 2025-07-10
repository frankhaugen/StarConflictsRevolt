namespace StarConflictsRevolt.Server.WebApi.Eventing;

public record MoveFleetEvent(Guid PlayerId, Guid FleetId, Guid FromPlanetId, Guid ToPlanetId) : IGameEvent;
namespace StarConflictsRevolt.Server.WebApi.Eventing;

public record BuildStructureEvent(Guid PlayerId, Guid PlanetId, string StructureType) : IGameEvent;
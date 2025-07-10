namespace StarConflictsRevolt.Server.Eventing;

public record BuildStructureEvent(Guid PlayerId, Guid PlanetId, string StructureType) : IGameEvent;
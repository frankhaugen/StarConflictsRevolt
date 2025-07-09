namespace StarConflictsRevolt.Server.Eventing;

public interface IGameEvent { }

// --- Concrete Game Events ---

public record MoveFleetEvent(Guid PlayerId, Guid FleetId, Guid FromPlanetId, Guid ToPlanetId) : IGameEvent;

public record BuildStructureEvent(Guid PlayerId, Guid PlanetId, string StructureType) : IGameEvent;

public record AttackEvent(Guid PlayerId, Guid AttackerFleetId, Guid DefenderFleetId, Guid LocationPlanetId) : IGameEvent;

public record DiplomacyEvent(Guid PlayerId, Guid TargetPlayerId, string ProposalType, string? Message) : IGameEvent;
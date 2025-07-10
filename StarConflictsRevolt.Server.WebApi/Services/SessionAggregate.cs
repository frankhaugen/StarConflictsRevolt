using StarConflictsRevolt.Server.Core.Models;
using StarConflictsRevolt.Server.Eventing;
using StarConflictsRevolt.Server.Core.Enums;
using Microsoft.Extensions.Logging;

namespace StarConflictsRevolt.Server.Services;

public class SessionAggregate
{
    private readonly ILogger<SessionAggregate> _logger;

    public Guid SessionId { get; set; }
    public World World { get; set; }
    public int Version { get; set; }
    public List<IGameEvent> UncommittedEvents { get; } = new();

    public SessionAggregate(Guid sessionId, World initialWorld, ILogger<SessionAggregate> logger)
    {
        SessionId = sessionId;
        World = initialWorld;
        Version = 0;
        _logger = logger;
    }

    public void Apply(IGameEvent e)
    {
        _logger.LogInformation("Applying event {EventType} to session {SessionId}, version {Version}", e.GetType().Name, SessionId, Version);
        
        switch (e)
        {
            case MoveFleetEvent move:
                // Move the fleet from one planet to another
                foreach (var system in World.Galaxy.StarSystems)
                {
                    var fromPlanet = system.Planets.FirstOrDefault(p => p.Id == move.FromPlanetId);
                    var toPlanet = system.Planets.FirstOrDefault(p => p.Id == move.ToPlanetId);
                    if (fromPlanet != null && toPlanet != null)
                    {
                        var fleet = fromPlanet.Fleets.FirstOrDefault(f => f.Id == move.FleetId);
                        if (fleet != null)
                        {
                            fromPlanet.Fleets.Remove(fleet);
                            fleet = fleet with { LocationPlanetId = toPlanet.Id };
                            toPlanet.Fleets.Add(fleet);
                            _logger.LogInformation("Moved fleet {FleetId} from planet {FromPlanet} to planet {ToPlanet}", 
                                move.FleetId, move.FromPlanetId, move.ToPlanetId);
                        }
                    }
                }
                break;
            case BuildStructureEvent build:
                // Add a structure to the planet's list of structures
                foreach (var system in World.Galaxy.StarSystems)
                {
                    var planet = system.Planets.FirstOrDefault(p => p.Id == build.PlanetId);
                    if (planet != null)
                    {
                        // Create a new Structure with the given variant
                        var structure = new Structure(
                            Enum.TryParse<StructureVariant>(build.StructureType, out var variant) ? variant : StructureVariant.ConstructionYard,
                            planet
                        );
                        planet.Structures.Add(structure);
                        _logger.LogInformation("Added structure {StructureType} to planet {PlanetId} in session {SessionId}", 
                            build.StructureType, build.PlanetId, SessionId);
                    }
                    else
                    {
                        _logger.LogWarning("Planet {PlanetId} not found for BuildStructureEvent in session {SessionId}", 
                            build.PlanetId, SessionId);
                    }
                }
                break;
            case AttackEvent attack:
                // Simple combat: remove defender fleet if found
                foreach (var system in World.Galaxy.StarSystems)
                {
                    var planet = system.Planets.FirstOrDefault(p => p.Id == attack.LocationPlanetId);
                    if (planet != null)
                    {
                        var defender = planet.Fleets.FirstOrDefault(f => f.Id == attack.DefenderFleetId);
                        if (defender != null)
                        {
                            planet.Fleets.Remove(defender);
                            _logger.LogInformation("Removed defender fleet {DefenderFleetId} from planet {PlanetId} in session {SessionId}", 
                                attack.DefenderFleetId, attack.LocationPlanetId, SessionId);
                        }
                    }
                }
                break;
            case DiplomacyEvent diplo:
                // For demonstration, just log or store the proposal type/message (no-op)
                // In a real implementation, update player relations, etc.
                _logger.LogInformation("Diplomacy event processed for session {SessionId}", SessionId);
                break;
        }
        UncommittedEvents.Add(e);
        Version++;
        _logger.LogInformation("Event {EventType} applied successfully to session {SessionId}, new version: {Version}", 
            e.GetType().Name, SessionId, Version);
    }

    public void ReplayEvents(IEnumerable<IGameEvent> events)
    {
        foreach (var e in events)
            Apply(e);
        UncommittedEvents.Clear();
    }

    public void LoadFromSnapshot(World snapshot, int version)
    {
        World = snapshot;
        Version = version;
        UncommittedEvents.Clear();
    }
} 
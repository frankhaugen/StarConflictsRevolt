using StarConflictsRevolt.Server.Core.Models;
using StarConflictsRevolt.Server.Eventing;

namespace StarConflictsRevolt.Server.Services;

public class SessionAggregate
{
    public Guid SessionId { get; set; }
    public World World { get; set; }
    public int Version { get; set; }
    public List<IGameEvent> UncommittedEvents { get; } = new();

    public SessionAggregate(Guid sessionId, World initialWorld)
    {
        SessionId = sessionId;
        World = initialWorld;
        Version = 0;
    }

    public void Apply(IGameEvent e)
    {
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
                        // For simplicity, create a new StructureType with a random Guid and the given variant name
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
                        }
                    }
                }
                break;
            case DiplomacyEvent diplo:
                // For demonstration, just log or store the proposal type/message (no-op)
                // In a real implementation, update player relations, etc.
                break;
        }
        UncommittedEvents.Add(e);
        Version++;
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
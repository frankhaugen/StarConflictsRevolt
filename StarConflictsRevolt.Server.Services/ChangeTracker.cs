using StarConflictsRevolt.Clients.Models;
using StarConflictsRevolt.Server.Core.Models;

namespace StarConflictsRevolt.Server.Services;

public static class ChangeTracker
{
    public static List<GameObjectUpdate> ComputeDeltas(World oldWorld, World newWorld)
    {
        var updates = new List<GameObjectUpdate>();

        // Compare World
        if (oldWorld.Id != newWorld.Id || !Equals(oldWorld.Galaxy, newWorld.Galaxy))
        {
            updates.Add(new GameObjectUpdate(newWorld.Id, UpdateType.Changed, newWorld));
        }

        // Compare Galaxies
        var oldGalaxy = oldWorld.Galaxy;
        var newGalaxy = newWorld.Galaxy;
        if (oldGalaxy.Id != newGalaxy.Id)
        {
            updates.Add(new GameObjectUpdate(newGalaxy.Id, UpdateType.Changed, newGalaxy));
        }

        // Compare StarSystems
        var oldSystems = oldGalaxy.StarSystems.ToDictionary(s => s.Id);
        var newSystems = newGalaxy.StarSystems.ToDictionary(s => s.Id);
        foreach (var (id, newSystem) in newSystems)
        {
            if (!oldSystems.TryGetValue(id, out var oldSystem))
            {
                updates.Add(new GameObjectUpdate(newSystem.Id, UpdateType.Added, newSystem));
                continue;
            }
            if (!Equals(oldSystem, newSystem))
            {
                updates.Add(new GameObjectUpdate(newSystem.Id, UpdateType.Changed, newSystem));
            }

            // Compare Planets
            var oldPlanets = oldSystem.Planets.ToDictionary(p => p.Id);
            var newPlanets = newSystem.Planets.ToDictionary(p => p.Id);
            foreach (var (pid, newPlanet) in newPlanets)
            {
                if (!oldPlanets.TryGetValue(pid, out var oldPlanet))
                {
                    updates.Add(new GameObjectUpdate(newPlanet.Id, UpdateType.Added, newPlanet));
                    continue;
                }
                if (!Equals(oldPlanet, newPlanet))
                {
                    updates.Add(new GameObjectUpdate(newPlanet.Id, UpdateType.Changed, newPlanet));
                }

                // Compare Fleets
                var oldFleets = oldPlanet.Fleets.ToDictionary(f => f.Id);
                var newFleets = newPlanet.Fleets.ToDictionary(f => f.Id);
                foreach (var (fid, newFleet) in newFleets)
                {
                    if (!oldFleets.TryGetValue(fid, out var oldFleet))
                    {
                        updates.Add(new GameObjectUpdate(newFleet.Id, UpdateType.Added, newFleet));
                        continue;
                    }
                    if (!Equals(oldFleet, newFleet))
                    {
                        updates.Add(new GameObjectUpdate(newFleet.Id, UpdateType.Changed, newFleet));
                    }
                }
                foreach (var (fid, oldFleet) in oldFleets)
                {
                    if (!newFleets.ContainsKey(fid))
                    {
                        updates.Add(new GameObjectUpdate(oldFleet.Id, UpdateType.Removed, null));
                    }
                }
            }
            foreach (var (pid, oldPlanet) in oldPlanets)
            {
                if (!newPlanets.ContainsKey(pid))
                {
                    updates.Add(new GameObjectUpdate(oldPlanet.Id, UpdateType.Removed, null));
                }
            }
        }
        foreach (var (id, oldSystem) in oldSystems)
        {
            if (!newSystems.ContainsKey(id))
            {
                updates.Add(new GameObjectUpdate(oldSystem.Id, UpdateType.Removed, null));
            }
        }
        return updates;
    }
} 
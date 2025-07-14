using StarConflictsRevolt.Clients.Models;
using StarConflictsRevolt.Server.WebApi.Models;

namespace StarConflictsRevolt.Server.WebApi.Services;

public static class ChangeTracker
{
    public static List<GameObjectUpdate> ComputeDeltas(World oldWorld, World newWorld)
    {
        var updates = new List<GameObjectUpdate>();

        // Compare World
        if (oldWorld.Id != newWorld.Id || !Equals(oldWorld.Galaxy, newWorld.Galaxy)) updates.Add(GameObjectUpdate.Update(newWorld.Id, newWorld));

        // Compare Galaxies
        var oldGalaxy = oldWorld.Galaxy;
        var newGalaxy = newWorld.Galaxy;
        if (oldGalaxy.Id != newGalaxy.Id) updates.Add(GameObjectUpdate.Update(newGalaxy.Id, newGalaxy));

        // Compare StarSystems
        var oldSystems = oldGalaxy.StarSystems.ToDictionary(s => s.Id);
        var newSystems = newGalaxy.StarSystems.ToDictionary(s => s.Id);
        foreach (var (id, newSystem) in newSystems)
        {
            if (!oldSystems.TryGetValue(id, out var oldSystem))
            {
                updates.Add(GameObjectUpdate.Create(newSystem.Id, newSystem));
                continue;
            }

            if (!Equals(oldSystem, newSystem)) updates.Add(GameObjectUpdate.Update(newSystem.Id, newSystem));

            // Compare Planets
            var oldPlanets = oldSystem.Planets.ToDictionary(p => p.Id);
            var newPlanets = newSystem.Planets.ToDictionary(p => p.Id);
            foreach (var (pid, newPlanet) in newPlanets)
            {
                if (!oldPlanets.TryGetValue(pid, out var oldPlanet))
                {
                    updates.Add(GameObjectUpdate.Create(newPlanet.Id, newPlanet));
                    continue;
                }

                // Check if planet has changed (e.g., structures added)
                var planetChanged = !Equals(oldPlanet, newPlanet);
                if (planetChanged) updates.Add(GameObjectUpdate.Update(newPlanet.Id, newPlanet));

                // Compare Fleets
                var oldFleets = oldPlanet.Fleets.ToDictionary(f => f.Id);
                var newFleets = newPlanet.Fleets.ToDictionary(f => f.Id);
                foreach (var (fid, newFleet) in newFleets)
                {
                    if (!oldFleets.TryGetValue(fid, out var oldFleet))
                    {
                        updates.Add(GameObjectUpdate.Create(newFleet.Id, newFleet));
                        continue;
                    }

                    if (!Equals(oldFleet, newFleet)) updates.Add(GameObjectUpdate.Update(newFleet.Id, newFleet));
                }

                foreach (var (fid, oldFleet) in oldFleets)
                    if (!newFleets.ContainsKey(fid))
                        updates.Add(GameObjectUpdate.Delete(oldFleet.Id));

                // Compare Structures
                var oldStructures = oldPlanet.Structures.ToDictionary(s => s.Id);
                var newStructures = newPlanet.Structures.ToDictionary(s => s.Id);
                foreach (var (sid, newStructure) in newStructures)
                {
                    if (!oldStructures.TryGetValue(sid, out var oldStructure))
                    {
                        updates.Add(GameObjectUpdate.Create(newStructure.Id, newStructure));
                        continue;
                    }

                    if (!Equals(oldStructure, newStructure)) updates.Add(GameObjectUpdate.Update(newStructure.Id, newStructure));
                }

                foreach (var (sid, oldStructure) in oldStructures)
                    if (!newStructures.ContainsKey(sid))
                        updates.Add(GameObjectUpdate.Delete(oldStructure.Id));
            }
        }

        foreach (var (id, oldSystem) in oldSystems)
            if (!newSystems.ContainsKey(id))
                updates.Add(GameObjectUpdate.Delete(oldSystem.Id));

        return updates;
    }
}
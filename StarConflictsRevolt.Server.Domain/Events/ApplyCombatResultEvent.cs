using StarConflictsRevolt.Server.EventStorage.Abstractions;
using StarConflictsRevolt.Server.Domain.Fleets;
using StarConflictsRevolt.Server.Domain.Planets;
using StarConflictsRevolt.Server.Domain.Stars;
using WorldState = StarConflictsRevolt.Server.Domain.World.World;

namespace StarConflictsRevolt.Server.Domain.Events;

/// <summary>
/// Applies the result of a combat resolution (from the Combat module) to the world:
/// removes destroyed ships, updates health of survivors, removes empty fleets.
/// </summary>
public record ApplyCombatResultEvent(
    Guid PlayerId,
    Guid AttackerFleetId,
    Guid DefenderFleetId,
    Guid LocationPlanetId,
    IReadOnlyList<Guid> AttackerDestroyedShipIds,
    IReadOnlyList<Guid> DefenderDestroyedShipIds,
    IReadOnlyDictionary<Guid, int> AttackerSurvivorHealths,
    IReadOnlyDictionary<Guid, int> DefenderSurvivorHealths
) : IGameEvent
{
    public void ApplyTo(object world, ILogger logger)
    {
        var w = (WorldState)world;
        Planet? planet = null;
        StarSystem? containingSystem = null;
        Fleet? attackerFleet = null;
        Fleet? defenderFleet = null;

        foreach (var system in w.Galaxy.StarSystems)
        {
            planet = system.Planets.FirstOrDefault(p => p.Id == LocationPlanetId);
            if (planet != null)
            {
                containingSystem = system;
                attackerFleet = planet.Fleets.FirstOrDefault(f => f.Id == AttackerFleetId);
                defenderFleet = planet.Fleets.FirstOrDefault(f => f.Id == DefenderFleetId);
                break;
            }
        }

        if (planet == null || containingSystem == null)
        {
            logger.LogWarning("Planet {PlanetId} not found for ApplyCombatResultEvent", LocationPlanetId);
            return;
        }
        if (attackerFleet == null || defenderFleet == null)
        {
            logger.LogWarning("Attacker or defender fleet not found for ApplyCombatResultEvent");
            return;
        }

        var attackerShips = attackerFleet.Ships
            .Where(s => !AttackerDestroyedShipIds.Contains(s.Id))
            .Select(s => AttackerSurvivorHealths.TryGetValue(s.Id, out var h) ? s with { Health = Math.Max(0, h) } : s)
            .ToList();
        var defenderShips = defenderFleet.Ships
            .Where(s => !DefenderDestroyedShipIds.Contains(s.Id))
            .Select(s => DefenderSurvivorHealths.TryGetValue(s.Id, out var h) ? s with { Health = Math.Max(0, h) } : s)
            .ToList();

        logger.LogInformation("Combat result applied: Attacker {AttackerFleetId} {AttackerSurvived} survivors, Defender {DefenderFleetId} {DefenderSurvived} survivors",
            AttackerFleetId, attackerShips.Count, DefenderFleetId, defenderShips.Count);

        var updatedFleets = planet.Fleets
            .Where(f => f.Id != AttackerFleetId && f.Id != DefenderFleetId)
            .ToList();

        if (attackerShips.Count > 0)
            updatedFleets.Add(attackerFleet with { Ships = attackerShips });
        if (defenderShips.Count > 0)
            updatedFleets.Add(defenderFleet with { Ships = defenderShips });

        var updatedPlanet = planet with { Fleets = updatedFleets };
        var planetIndex = containingSystem.Planets.FindIndex(p => p.Id == LocationPlanetId);
        if (planetIndex >= 0)
            containingSystem.Planets[planetIndex] = updatedPlanet;
    }
}

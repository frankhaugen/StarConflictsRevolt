using StarConflictsRevolt.Server.WebApi.Enums;
using StarConflictsRevolt.Server.WebApi.Models;

namespace StarConflictsRevolt.Server.WebApi.Eventing;

public record AttackEvent(Guid PlayerId, Guid AttackerFleetId, Guid DefenderFleetId, Guid LocationPlanetId) : IGameEvent
{
    public void ApplyTo(World world, Microsoft.Extensions.Logging.ILogger logger)
    {
        // Find the planet and its containing system
        Planet? planet = null;
        StarSystem? containingSystem = null;
        Fleet? attackerFleet = null;
        Fleet? defenderFleet = null;

        foreach (var system in world.Galaxy.StarSystems)
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
            logger.LogWarning("Planet {PlanetId} not found for AttackEvent", LocationPlanetId);
            return;
        }

        if (attackerFleet == null)
        {
            logger.LogWarning("Attacker fleet {AttackerFleetId} not found on planet {PlanetId}", AttackerFleetId, LocationPlanetId);
            return;
        }

        if (defenderFleet == null)
        {
            logger.LogWarning("Defender fleet {DefenderFleetId} not found on planet {PlanetId}", DefenderFleetId, LocationPlanetId);
            return;
        }

        // Validate ownership
        if (attackerFleet.OwnerId != PlayerId)
        {
            logger.LogWarning("Player {PlayerId} does not own attacker fleet {AttackerFleetId}", PlayerId, AttackerFleetId);
            return;
        }

        // Calculate combat power
        var attackerPower = CalculateFleetPower(attackerFleet);
        var defenderPower = CalculateFleetPower(defenderFleet);

        logger.LogInformation("Combat initiated: Attacker power {AttackerPower} vs Defender power {DefenderPower}", 
            attackerPower, defenderPower);

        // Resolve combat
        var (attackerDamage, defenderDamage) = ResolveCombat(attackerPower, defenderPower);

        // Apply damage to fleets
        var updatedAttackerFleet = ApplyDamageToFleet(attackerFleet, defenderDamage);
        var updatedDefenderFleet = ApplyDamageToFleet(defenderFleet, attackerDamage);

        // Remove old fleets from the planet
        var updatedFleets = planet.Fleets.Where(f => f.Id != attackerFleet.Id && f.Id != defenderFleet.Id).ToList();

        // Add updated fleets back (if they survived)
        if (updatedAttackerFleet.Ships.Any(s => s.Health > 0))
        {
            updatedFleets.Add(updatedAttackerFleet);
            logger.LogInformation("Attacker fleet {AttackerFleetId} survived with {RemainingShips} ships", 
                AttackerFleetId, updatedAttackerFleet.Ships.Count(s => s.Health > 0));
        }
        else
        {
            logger.LogInformation("Attacker fleet {AttackerFleetId} was destroyed", AttackerFleetId);
        }

        if (updatedDefenderFleet.Ships.Any(s => s.Health > 0))
        {
            updatedFleets.Add(updatedDefenderFleet);
            logger.LogInformation("Defender fleet {DefenderFleetId} survived with {RemainingShips} ships", 
                DefenderFleetId, updatedDefenderFleet.Ships.Count(s => s.Health > 0));
        }
        else
        {
            logger.LogInformation("Defender fleet {DefenderFleetId} was destroyed", DefenderFleetId);
        }

        // Replace the planet in the system's planet list with updated fleets
        var updatedPlanet = planet with { Fleets = updatedFleets };
        var planetIndex = containingSystem.Planets.FindIndex(p => p.Id == LocationPlanetId);
        if (planetIndex >= 0)
        {
            containingSystem.Planets[planetIndex] = updatedPlanet;
        }
    }

    private static int CalculateFleetPower(Fleet fleet)
    {
        return fleet.Ships.Sum(ship => ship.AttackPower + ship.DefensePower);
    }

    private static (int attackerDamage, int defenderDamage) ResolveCombat(int attackerPower, int defenderPower)
    {
        var random = new Random();
        
        // Base damage calculation
        var attackerDamage = (int)(attackerPower * (0.5 + random.NextDouble() * 0.5)); // 50-100% of power
        var defenderDamage = (int)(defenderPower * (0.5 + random.NextDouble() * 0.5)); // 50-100% of power

        // Add some randomness
        attackerDamage += random.Next(-10, 11);
        defenderDamage += random.Next(-10, 11);

        return (Math.Max(0, attackerDamage), Math.Max(0, defenderDamage));
    }

    private static Fleet ApplyDamageToFleet(Fleet fleet, int totalDamage)
    {
        if (totalDamage <= 0) return fleet;

        var ships = fleet.Ships.ToList();
        var damagePerShip = totalDamage / ships.Count;
        var remainingDamage = totalDamage % ships.Count;

        var updatedShips = new List<Ship>();
        foreach (var ship in ships)
        {
            var shipDamage = damagePerShip + (remainingDamage > 0 ? 1 : 0);
            remainingDamage = Math.Max(0, remainingDamage - 1);

            var newHealth = Math.Max(0, ship.Health - shipDamage);
            var updatedShip = ship with { Health = newHealth };
            updatedShips.Add(updatedShip);
        }

        return fleet with { Ships = updatedShips };
    }
}
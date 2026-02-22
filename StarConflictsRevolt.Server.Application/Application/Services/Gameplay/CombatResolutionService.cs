using StarConflictsRevolt.Server.Domain.Combat;
using StarConflictsRevolt.Server.Domain.Events;
using StarConflictsRevolt.Server.Domain.Fleets;
using StarConflictsRevolt.Server.Domain.Planets;
using StarConflictsRevolt.Server.Combat;
using WorldState = StarConflictsRevolt.Server.Domain.World.World;

namespace StarConflictsRevolt.Server.Application.Services.Gameplay;

/// <summary>
/// Resolves attack commands using the Combat module and produces an event that applies the result to the world.
/// </summary>
public class CombatResolutionService
{
    private readonly ICombatSimulator _combatSimulator;
    private readonly ILogger<CombatResolutionService> _logger;

    public CombatResolutionService(ICombatSimulator combatSimulator, ILogger<CombatResolutionService> logger)
    {
        _combatSimulator = combatSimulator;
        _logger = logger;
    }

    /// <summary>
    /// Runs fleet combat simulation and returns an event to apply the result to the world.
    /// Returns null if attacker/defender/planet not found or combat cannot be run.
    /// </summary>
    public async Task<ApplyCombatResultEvent?> ResolveAndCreateEventAsync(WorldState world, AttackEvent attackEvent, CancellationToken cancellationToken = default)
    {
        var (attacker, defender, planet) = FindFleetsAndPlanet(world, attackEvent.LocationPlanetId, attackEvent.AttackerFleetId, attackEvent.DefenderFleetId);
        if (attacker == null || defender == null || planet == null)
        {
            _logger.LogWarning("Cannot resolve combat: attacker, defender or planet not found");
            return null;
        }
        if (attacker.OwnerId != attackEvent.PlayerId)
        {
            _logger.LogWarning("Player {PlayerId} does not own attacker fleet {FleetId}", attackEvent.PlayerId, attackEvent.AttackerFleetId);
            return null;
        }

        _logger.LogInformation("Resolving combat: Attacker {AttackerId} vs Defender {DefenderId} at {PlanetName}",
            attacker.Id, defender.Id, planet.Name);

        CombatResult result;
        try
        {
            result = await _combatSimulator.SimulateFleetCombatAsync(attacker, defender, planet);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Combat simulation failed");
            return null;
        }

        var attackerDestroyed = result.AttackerLosses.Select(s => s.Id).ToList();
        var defenderDestroyed = result.DefenderLosses.Select(s => s.Id).ToList();

        return new ApplyCombatResultEvent(
            attackEvent.PlayerId,
            attackEvent.AttackerFleetId,
            attackEvent.DefenderFleetId,
            attackEvent.LocationPlanetId,
            attackerDestroyed,
            defenderDestroyed,
            result.AttackerSurvivorHealths,
            result.DefenderSurvivorHealths
        );
    }

    private static (Fleet? attacker, Fleet? defender, Planet? planet) FindFleetsAndPlanet(
        WorldState world,
        Guid locationPlanetId,
        Guid attackerFleetId,
        Guid defenderFleetId)
    {
        Fleet? attacker = null;
        Fleet? defender = null;
        Planet? planet = null;

        foreach (var system in world.Galaxy.StarSystems)
        {
            planet = system.Planets.FirstOrDefault(p => p.Id == locationPlanetId);
            if (planet == null) continue;

            attacker = planet.Fleets.FirstOrDefault(f => f.Id == attackerFleetId);
            defender = planet.Fleets.FirstOrDefault(f => f.Id == defenderFleetId);
            if (attacker != null && defender != null)
                break;
        }

        return (attacker, defender, planet);
    }
}

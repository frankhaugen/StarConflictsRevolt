using StarConflictsRevolt.Server.WebApi.Models;
using StarConflictsRevolt.Server.WebApi.Models.Combat;

namespace StarConflictsRevolt.Server.WebApi.Services.Combat;

public class CombatSimulatorService : ICombatSimulator
{
    private readonly IFleetCombatSimulator _fleetCombatSimulator;
    private readonly IPlanetaryCombatSimulator _planetaryCombatSimulator;
    private readonly IDeathStarRunSimulator _deathStarRunSimulator;
    private readonly IMissionSimulator _missionSimulator;
    private readonly ILogger<CombatSimulatorService> _logger;

    public CombatSimulatorService(
        IFleetCombatSimulator fleetCombatSimulator,
        IPlanetaryCombatSimulator planetaryCombatSimulator,
        IDeathStarRunSimulator deathStarRunSimulator,
        IMissionSimulator missionSimulator,
        ILogger<CombatSimulatorService> logger)
    {
        _fleetCombatSimulator = fleetCombatSimulator;
        _planetaryCombatSimulator = planetaryCombatSimulator;
        _deathStarRunSimulator = deathStarRunSimulator;
        _missionSimulator = missionSimulator;
        _logger = logger;
    }

    public async Task<CombatResult> SimulateFleetCombatAsync(Fleet attacker, Fleet defender, Planet? location = null)
    {
        _logger.LogInformation("Starting fleet combat simulation. Attacker: {AttackerId}, Defender: {DefenderId}, Location: {Location}",
            attacker.Id, defender.Id, location?.Name ?? "Space");

        try
        {
            var result = _fleetCombatSimulator.SimulateFleetCombat(attacker, defender, location);
            
            _logger.LogInformation("Fleet combat simulation completed. Result: {AttackerVictory}, Rounds: {Rounds}, Duration: {Duration}",
                result.AttackerVictory, result.RoundsFought, result.Duration);

            return await Task.FromResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during fleet combat simulation");
            throw;
        }
    }

    public async Task<CombatResult> SimulatePlanetaryCombatAsync(Fleet attacker, Planet defender)
    {
        _logger.LogInformation("Starting planetary combat simulation. Attacker: {AttackerId}, Defender: {DefenderId}",
            attacker.Id, defender.Id);

        try
        {
            var result = _planetaryCombatSimulator.SimulatePlanetaryCombat(attacker, defender);
            
            _logger.LogInformation("Planetary combat simulation completed. Result: {AttackerVictory}, Rounds: {Rounds}, Duration: {Duration}",
                result.AttackerVictory, result.RoundsFought, result.Duration);

            return await Task.FromResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during planetary combat simulation");
            throw;
        }
    }

    public async Task<CombatResult> SimulateDeathStarRunAsync(Fleet attacker, DeathStar defender)
    {
        _logger.LogInformation("Starting Death Star run simulation. Attacker: {AttackerId}, Death Star: {DeathStarId}",
            attacker.Id, defender.Id);

        try
        {
            var result = _deathStarRunSimulator.SimulateDeathStarRun(attacker, defender);
            
            _logger.LogInformation("Death Star run simulation completed. Result: {AttackerVictory}, Duration: {Duration}",
                result.AttackerVictory, result.Duration);

            return await Task.FromResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Death Star run simulation");
            throw;
        }
    }

    public async Task<CombatResult> SimulateMissionAsync(Mission mission, Character agent, Planet target)
    {
        _logger.LogInformation("Starting mission simulation. Mission: {MissionId}, Agent: {AgentId}, Target: {TargetId}",
            mission.Id, agent.Id, target.Id);

        try
        {
            var result = _missionSimulator.SimulateMission(mission, agent, target);
            
            _logger.LogInformation("Mission simulation completed. Result: {AttackerVictory}, Duration: {Duration}",
                result.AttackerVictory, result.Duration);

            return await Task.FromResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during mission simulation");
            throw;
        }
    }
} 
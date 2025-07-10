using StarConflictsRevolt.Server.WebApi.Enums;
using StarConflictsRevolt.Server.WebApi.Eventing;
using StarConflictsRevolt.Server.WebApi.Models;

namespace StarConflictsRevolt.Server.WebApi.Services;

public class AiController : PlayerController
{
    private readonly Random _random = new();
    private readonly ILogger<AiController> _logger;

    public AiController(ILogger<AiController> logger)
    {
        _logger = logger;
    }

    public override List<IGameEvent> GenerateCommands(World world)
    {
        var commands = new List<IGameEvent>();
        
        // Find AI's fleets
        var aiFleets = world.Galaxy.StarSystems
            .SelectMany(s => s.Planets)
            .SelectMany(p => p.Fleets)
            .Where(f => f.Id == PlayerId) // Simple ownership check
            .ToList();

        // Find AI's planets
        var aiPlanets = world.Galaxy.StarSystems
            .SelectMany(s => s.Planets)
            .Where(p => p.Fleets.Any(f => f.Id == PlayerId))
            .ToList();

        // Simple AI logic: move fleets randomly, build structures occasionally
        foreach (var fleet in aiFleets)
        {
            if (_random.Next(100) < 30) // 30% chance to move
            {
                var allPlanets = world.Galaxy.StarSystems.SelectMany(s => s.Planets).ToList();
                var targetPlanet = allPlanets[_random.Next(allPlanets.Count)];
                
                if (fleet.LocationPlanetId != targetPlanet.Id)
                {
                    commands.Add(new MoveFleetEvent(PlayerId, fleet.Id, fleet.LocationPlanetId ?? Guid.Empty, targetPlanet.Id));
                }
            }
        }

        // Build structures on AI planets occasionally
        foreach (var planet in aiPlanets)
        {
            if (_random.Next(100) < 20) // 20% chance to build
            {
                var structureTypes = Enum.GetValues<StructureVariant>();
                var structureType = structureTypes[_random.Next(structureTypes.Length)];
                
                commands.Add(new BuildStructureEvent(PlayerId, planet.Id, structureType.ToString()));
            }
        }

        // Attack enemy fleets occasionally
        var enemyFleets = world.Galaxy.StarSystems
            .SelectMany(s => s.Planets)
            .SelectMany(p => p.Fleets)
            .Where(f => f.Id != PlayerId)
            .ToList();

        if (enemyFleets.Any() && aiFleets.Any() && _random.Next(100) < 15) // 15% chance to attack
        {
            var attacker = aiFleets[_random.Next(aiFleets.Count)];
            var defender = enemyFleets[_random.Next(enemyFleets.Count)];
            var location = world.Galaxy.StarSystems
                .SelectMany(s => s.Planets)
                .FirstOrDefault(p => p.Fleets.Contains(defender));

            if (location != null)
            {
                commands.Add(new AttackEvent(PlayerId, attacker.Id, defender.Id, location.Id));
            }
        }

        _logger.LogInformation("AI Player {PlayerId} generated {CommandCount} commands", PlayerId, commands.Count);
        return commands;
    }
}
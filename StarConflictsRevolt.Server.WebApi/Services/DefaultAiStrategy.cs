using StarConflictsRevolt.Server.WebApi.Enums;
using StarConflictsRevolt.Server.WebApi.Eventing;
using StarConflictsRevolt.Server.WebApi.Models;

namespace StarConflictsRevolt.Server.WebApi.Services;

public class DefaultAiStrategy : IAiStrategy
{
    private readonly Random _random = new();
    public List<IGameEvent> GenerateCommands(Guid playerId, World world, ILogger logger)
    {
        var commands = new List<IGameEvent>();
        var aiFleets = world.Galaxy.StarSystems
            .SelectMany(s => s.Planets)
            .SelectMany(p => p.Fleets)
            .Where(f => f.Id == playerId)
            .ToList();
        var aiPlanets = world.Galaxy.StarSystems
            .SelectMany(s => s.Planets)
            .Where(p => p.Fleets.Any(f => f.Id == playerId))
            .ToList();
        foreach (var fleet in aiFleets)
        {
            if (_random.Next(100) < 30)
            {
                var allPlanets = world.Galaxy.StarSystems.SelectMany(s => s.Planets).ToList();
                var targetPlanet = allPlanets[_random.Next(allPlanets.Count)];
                if (fleet.LocationPlanetId != targetPlanet.Id)
                {
                    commands.Add(new MoveFleetEvent(playerId, fleet.Id, fleet.LocationPlanetId ?? Guid.Empty, targetPlanet.Id));
                }
            }
        }
        foreach (var planet in aiPlanets)
        {
            if (_random.Next(100) < 20)
            {
                var structureTypes = Enum.GetValues<StructureVariant>();
                var structureType = structureTypes[_random.Next(structureTypes.Length)];
                commands.Add(new BuildStructureEvent(playerId, planet.Id, structureType.ToString()));
            }
        }
        var enemyFleets = world.Galaxy.StarSystems
            .SelectMany(s => s.Planets)
            .SelectMany(p => p.Fleets)
            .Where(f => f.Id != playerId)
            .ToList();
        if (enemyFleets.Any() && aiFleets.Any() && _random.Next(100) < 15)
        {
            var attacker = aiFleets[_random.Next(aiFleets.Count)];
            var defender = enemyFleets[_random.Next(enemyFleets.Count)];
            var location = world.Galaxy.StarSystems
                .SelectMany(s => s.Planets)
                .FirstOrDefault(p => p.Fleets.Contains(defender));
            if (location != null)
            {
                commands.Add(new AttackEvent(playerId, attacker.Id, defender.Id, location.Id));
            }
        }
        logger?.LogInformation("AI Player {PlayerId} generated {CommandCount} commands", playerId, commands.Count);
        return commands;
    }
}
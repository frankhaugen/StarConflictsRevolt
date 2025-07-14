using StarConflictsRevolt.Server.WebApi.Application.Services.AI;
using StarConflictsRevolt.Server.WebApi.Core.Domain.AI;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Enums;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Events;
using StarConflictsRevolt.Server.WebApi.Core.Domain.World;

namespace StarConflictsRevolt.Server.WebApi.Application.Services.Gameplay;

public class DefaultAiStrategy : BaseAiStrategy
{
    public DefaultAiStrategy(AiMemoryBank memoryBank) : base(memoryBank)
    {
    }

    public override List<IGameEvent> GenerateCommands(Guid playerId, World world, ILogger logger)
    {
        if (!CanAct(playerId, TimeSpan.FromSeconds(3)))
            return new List<IGameEvent>();

        RecordAction(playerId);
        return GenerateCommandsInternal(playerId, world, logger);
    }

    protected override void UpdateGoals(Guid playerId, World world)
    {
        // Random AI doesn't set specific goals
    }

    protected override List<AiDecision> GenerateDecisions(Guid playerId, World world)
    {
        var decisions = new List<AiDecision>();

        var aiFleets = GetPlayerFleets(playerId, world);
        var aiPlanets = GetPlayerPlanets(playerId, world);
        var enemyFleets = GetEnemyFleets(playerId, world);

        // Random behavior (original implementation)
        foreach (var fleet in aiFleets)
            if (_random.Next(100) < 30)
            {
                var allPlanets = world.Galaxy.StarSystems.SelectMany(s => s.Planets).ToList();
                var targetPlanet = allPlanets[_random.Next(allPlanets.Count)];

                if (fleet.LocationPlanetId != targetPlanet.Id)
                {
                    var score = _random.NextDouble() * 50 + 25; // 25-75 score
                    var decision = new AiDecision(AiDecisionType.MoveFleet, AiPriority.Medium, score,
                        $"Random move: fleet {fleet.Id} to {targetPlanet.Name}");
                    decision.AddParameter("FleetId", fleet.Id);
                    decision.AddParameter("FromPlanetId", fleet.LocationPlanetId ?? Guid.Empty);
                    decision.AddParameter("ToPlanetId", targetPlanet.Id);
                    decisions.Add(decision);
                }
            }

        foreach (var planet in aiPlanets)
            if (_random.Next(100) < 20)
            {
                var structureTypes = Enum.GetValues<StructureVariant>();
                var structureType = structureTypes[_random.Next(structureTypes.Length)];

                var score = _random.NextDouble() * 40 + 30; // 30-70 score
                var decision = new AiDecision(AiDecisionType.BuildStructure, AiPriority.Medium, score,
                    $"Random build: {structureType} on {planet.Name}");
                decision.AddParameter("PlanetId", planet.Id);
                decision.AddParameter("StructureType", structureType.ToString());
                decisions.Add(decision);
            }

        if (enemyFleets.Any() && aiFleets.Any() && _random.Next(100) < 15)
        {
            var attacker = aiFleets[_random.Next(aiFleets.Count)];
            var defender = enemyFleets[_random.Next(enemyFleets.Count)];
            var location = world.Galaxy.StarSystems
                .SelectMany(s => s.Planets)
                .FirstOrDefault(p => p.Fleets.Contains(defender));

            if (location != null)
            {
                var score = _random.NextDouble() * 60 + 40; // 40-100 score
                var decision = new AiDecision(AiDecisionType.Attack, AiPriority.High, score,
                    $"Random attack: fleet {attacker.Id} attacks {defender.Id} at {location.Name}");
                decision.AddParameter("AttackerFleetId", attacker.Id);
                decision.AddParameter("DefenderFleetId", defender.Id);
                decision.AddParameter("LocationPlanetId", location.Id);
                decisions.Add(decision);
            }
        }

        return decisions;
    }
}
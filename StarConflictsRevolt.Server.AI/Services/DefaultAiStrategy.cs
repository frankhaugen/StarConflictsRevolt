using StarConflictsRevolt.Server.Domain.AI;
using StarConflictsRevolt.Server.Domain.Commands;
using StarConflictsRevolt.Server.Domain.Enums;
using StarConflictsRevolt.Server.EventStorage.Abstractions;
using WorldState = StarConflictsRevolt.Server.Domain.World.World;

namespace StarConflictsRevolt.Server.AI;

public class DefaultAiStrategy(AiMemoryBank memoryBank) : BaseAiStrategy(memoryBank)
{
    public override List<IGameCommand> GenerateCommands(Guid playerId, WorldState world, long clientTick, ILogger logger)
    {
        if (!CanAct(playerId, TimeSpan.FromSeconds(3)))
            return new List<IGameCommand>();

        RecordAction(playerId);
        return GenerateCommandsInternal(playerId, world, clientTick, logger);
    }

    protected override void UpdateGoals(Guid playerId, WorldState world)
    {
    }

    protected override List<AiDecision> GenerateDecisions(Guid playerId, WorldState world)
    {
        var decisions = new List<AiDecision>();

        var aiFleets = GetPlayerFleets(playerId, world);
        var aiPlanets = GetPlayerPlanets(playerId, world);
        var enemyFleets = GetEnemyFleets(playerId, world);

        foreach (var fleet in aiFleets)
            if (_random.Next(100) < 30)
            {
                var allPlanets = world.Galaxy.StarSystems.SelectMany(s => s.Planets).ToList();
                var targetPlanet = allPlanets[_random.Next(allPlanets.Count)];

                if (fleet.LocationPlanetId != targetPlanet.Id)
                {
                    var score = _random.NextDouble() * 50 + 25;
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

                var score = _random.NextDouble() * 40 + 30;
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
                var score = _random.NextDouble() * 60 + 40;
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

    public void AdjustDecisionWeights(double effectiveness)
    {
        Console.WriteLine($"Adjusting DefaultAiStrategy decision weights to {effectiveness}");
    }
}

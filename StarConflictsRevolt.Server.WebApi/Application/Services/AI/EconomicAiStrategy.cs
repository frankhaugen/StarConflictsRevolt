using StarConflictsRevolt.Server.WebApi.Eventing;
using StarConflictsRevolt.Server.WebApi.Models;

namespace StarConflictsRevolt.Server.WebApi.Services.AiStrategies;

public class EconomicAiStrategy : BaseAiStrategy
{
    public EconomicAiStrategy(AiMemoryBank memoryBank) : base(memoryBank)
    {
    }

    public override List<IGameEvent> GenerateCommands(Guid playerId, World world, ILogger logger)
    {
        if (!CanAct(playerId, TimeSpan.FromSeconds(5))) // Economic AI acts more slowly
            return new List<IGameEvent>();

        RecordAction(playerId);
        return GenerateCommandsInternal(playerId, world, logger);
    }

    protected override void UpdateGoals(Guid playerId, World world)
    {
        var aiPlanets = GetPlayerPlanets(playerId, world);
        var aiFleets = GetPlayerFleets(playerId, world);

        // Economic goals
        if (!_goals.Any(g => g.Type == AiGoalType.Build && !g.IsCompleted))
        {
            var buildGoal = new AiGoal(AiGoalType.Build, GoalTimeframe.ShortTerm,
                "Build economic structures", 95.0);
            _goals.Add(buildGoal);
        }

        if (aiPlanets.Count < 5 && !_goals.Any(g => g.Type == AiGoalType.Expand && !g.IsCompleted))
        {
            var expandGoal = new AiGoal(AiGoalType.Expand, GoalTimeframe.MediumTerm,
                "Expand to more planets for resources", 80.0);
            _goals.Add(expandGoal);
        }

        if (aiFleets.Count < 2 && !_goals.Any(g => g.Type == AiGoalType.Build && g.Description.Contains("fleet") && !g.IsCompleted))
        {
            var fleetGoal = new AiGoal(AiGoalType.Build, GoalTimeframe.ShortTerm,
                "Build defensive fleets", 70.0);
            _goals.Add(fleetGoal);
        }
    }

    protected override List<AiDecision> GenerateDecisions(Guid playerId, World world)
    {
        var decisions = new List<AiDecision>();

        var aiPlanets = GetPlayerPlanets(playerId, world);
        var aiFleets = GetPlayerFleets(playerId, world);
        var enemyFleets = GetEnemyFleets(playerId, world);

        // Economic AI prioritizes building and resource management
        decisions.AddRange(GenerateEconomicBuildingDecisions(playerId, aiPlanets, world));
        decisions.AddRange(GenerateDefensiveDecisions(playerId, aiFleets, enemyFleets, world));
        decisions.AddRange(GenerateExpansionDecisions(playerId, aiFleets, world));
        decisions.AddRange(GenerateFleetDecisions(playerId, aiFleets, world));

        return decisions;
    }

    private List<AiDecision> GenerateEconomicBuildingDecisions(Guid playerId, List<Planet> aiPlanets, World world)
    {
        var decisions = new List<AiDecision>();

        foreach (var planet in aiPlanets)
            // Economic AI builds resource-generating structures
            if (_random.Next(100) < 60) // 60% chance to build (high for economic)
            {
                var economicStructures = new[] { "Mine", "Refinery", "Construction Yard" };
                var structureType = economicStructures[_random.Next(economicStructures.Length)];

                var score = 80.0 + _random.NextDouble() * 20; // 80-100 score (high priority)
                var decision = new AiDecision(AiDecisionType.BuildStructure, AiPriority.High, score,
                    $"Build {structureType} on {planet.Name} for economic growth");
                decision.AddParameter("PlanetId", planet.Id);
                decision.AddParameter("StructureType", structureType);
                decisions.Add(decision);
            }

        return decisions;
    }

    private List<AiDecision> GenerateDefensiveDecisions(Guid playerId, List<Fleet> aiFleets, List<Fleet> enemyFleets, World world)
    {
        var decisions = new List<AiDecision>();

        if (!aiFleets.Any() || !enemyFleets.Any())
            return decisions;

        // Economic AI only attacks if threatened
        foreach (var aiFleet in aiFleets)
            if (_random.Next(100) < 25) // 25% chance to attack (low for economic)
            {
                // Only attack if enemy is close to our planets
                var aiPlanets = GetPlayerPlanets(playerId, world);
                var nearbyEnemies = enemyFleets.Where(f =>
                {
                    var location = world.Galaxy.StarSystems
                        .SelectMany(s => s.Planets)
                        .FirstOrDefault(p => p.Fleets.Contains(f));
                    return location != null && aiPlanets.Any(ap =>
                        world.Galaxy.StarSystems.Any(ss => ss.Planets.Contains(ap) && ss.Planets.Contains(location)));
                }).ToList();

                if (nearbyEnemies.Any())
                {
                    var defender = nearbyEnemies[_random.Next(nearbyEnemies.Count)];
                    var location = world.Galaxy.StarSystems
                        .SelectMany(s => s.Planets)
                        .FirstOrDefault(p => p.Fleets.Contains(defender));

                    if (location != null)
                    {
                        var score = 60.0 + _random.NextDouble() * 20; // 60-80 score (defensive)
                        var decision = new AiDecision(AiDecisionType.Attack, AiPriority.Medium, score,
                            $"Defensive attack on nearby enemy fleet {defender.Id} at {location.Name}");
                        decision.AddParameter("AttackerFleetId", aiFleet.Id);
                        decision.AddParameter("DefenderFleetId", defender.Id);
                        decision.AddParameter("LocationPlanetId", location.Id);
                        decisions.Add(decision);
                    }
                }
            }

        return decisions;
    }

    private List<AiDecision> GenerateExpansionDecisions(Guid playerId, List<Fleet> aiFleets, World world)
    {
        var decisions = new List<AiDecision>();

        if (!aiFleets.Any())
            return decisions;

        // Economic AI moves fleets to unclaimed planets for expansion
        foreach (var fleet in aiFleets)
            if (_random.Next(100) < 35) // 35% chance to move for expansion
            {
                var unclaimedPlanets = world.Galaxy.StarSystems
                    .SelectMany(s => s.Planets)
                    .Where(p => !p.Fleets.Any())
                    .ToList();

                if (unclaimedPlanets.Any())
                {
                    var targetPlanet = unclaimedPlanets[_random.Next(unclaimedPlanets.Count)];

                    if (fleet.LocationPlanetId != targetPlanet.Id)
                    {
                        var score = 65.0 + _random.NextDouble() * 20; // 65-85 score
                        var decision = new AiDecision(AiDecisionType.MoveFleet, AiPriority.Medium, score,
                            $"Move fleet {fleet.Id} to unclaimed planet {targetPlanet.Name} for expansion");
                        decision.AddParameter("FleetId", fleet.Id);
                        decision.AddParameter("FromPlanetId", fleet.LocationPlanetId ?? Guid.Empty);
                        decision.AddParameter("ToPlanetId", targetPlanet.Id);
                        decisions.Add(decision);
                    }
                }
            }

        return decisions;
    }

    protected override AiDecision? EvaluateFleetMovement(Guid playerId, Fleet fleet, World world)
    {
        // Economic AI moves less frequently and toward unclaimed planets
        if (_random.Next(100) < 25) // 25% chance (lower than base)
        {
            var unclaimedPlanets = world.Galaxy.StarSystems
                .SelectMany(s => s.Planets)
                .Where(p => !p.Fleets.Any())
                .ToList();

            var targetPlanet = unclaimedPlanets.Any()
                ? unclaimedPlanets[_random.Next(unclaimedPlanets.Count)]
                : world.Galaxy.StarSystems.SelectMany(s => s.Planets).ToList()[_random.Next(world.Galaxy.StarSystems.SelectMany(s => s.Planets).Count())];

            if (fleet.LocationPlanetId != targetPlanet.Id)
            {
                var score = 50.0 + _random.NextDouble() * 25; // 50-75 score
                var decision = new AiDecision(AiDecisionType.MoveFleet, AiPriority.Medium, score,
                    $"Economic move: fleet {fleet.Id} to {targetPlanet.Name}");
                decision.AddParameter("FleetId", fleet.Id);
                decision.AddParameter("FromPlanetId", fleet.LocationPlanetId ?? Guid.Empty);
                decision.AddParameter("ToPlanetId", targetPlanet.Id);
                return decision;
            }
        }

        return null;
    }

    protected override AiDecision? EvaluateBuilding(Guid playerId, Planet planet, World world)
    {
        // Economic AI builds more frequently and focuses on economic structures
        if (_random.Next(100) < 50) // 50% chance (higher than base)
        {
            var economicStructures = new[] { "Mine", "Refinery", "Construction Yard", "Shipyard" };
            var structureType = economicStructures[_random.Next(economicStructures.Length)];

            var score = 70.0 + _random.NextDouble() * 25; // 70-95 score (high priority)
            var decision = new AiDecision(AiDecisionType.BuildStructure, AiPriority.High, score,
                $"Economic build: {structureType} on {planet.Name}");
            decision.AddParameter("PlanetId", planet.Id);
            decision.AddParameter("StructureType", structureType);
            return decision;
        }

        return null;
    }

    protected override AiDecision? EvaluateCombatOpportunity(Guid playerId, Fleet aiFleet, List<Fleet> enemyFleets, World world)
    {
        // Economic AI rarely attacks
        if (_random.Next(100) < 10) // 10% chance (much lower than base)
        {
            var defender = enemyFleets[_random.Next(enemyFleets.Count)];
            var location = world.Galaxy.StarSystems
                .SelectMany(s => s.Planets)
                .FirstOrDefault(p => p.Fleets.Contains(defender));

            if (location != null)
            {
                var score = 40.0 + _random.NextDouble() * 30; // 40-70 score (lower priority)
                var decision = new AiDecision(AiDecisionType.Attack, AiPriority.Low, score,
                    $"Economic defensive attack: fleet {aiFleet.Id} attacks {defender.Id} at {location.Name}");
                decision.AddParameter("AttackerFleetId", aiFleet.Id);
                decision.AddParameter("DefenderFleetId", defender.Id);
                decision.AddParameter("LocationPlanetId", location.Id);
                return decision;
            }
        }

        return null;
    }
}
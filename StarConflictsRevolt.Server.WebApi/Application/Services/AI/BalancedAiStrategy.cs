using StarConflictsRevolt.Server.WebApi.Core.Domain.AI;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Events;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Fleets;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Planets;
using StarConflictsRevolt.Server.WebApi.Core.Domain.World;

namespace StarConflictsRevolt.Server.WebApi.Application.Services.AI;

public class BalancedAiStrategy : BaseAiStrategy
{
    private readonly Dictionary<Guid, AiStrategyPhase> _playerPhases = new();

    public BalancedAiStrategy(AiMemoryBank memoryBank) : base(memoryBank)
    {
    }

    public override List<IGameEvent> GenerateCommands(Guid playerId, World world, ILogger logger)
    {
        if (!CanAct(playerId, TimeSpan.FromSeconds(4))) // Balanced AI acts moderately
            return new List<IGameEvent>();

        RecordAction(playerId);
        return GenerateCommandsInternal(playerId, world, logger);
    }

    protected override void UpdateGoals(Guid playerId, World world)
    {
        var aiPlanets = GetPlayerPlanets(playerId, world);
        var aiFleets = GetPlayerFleets(playerId, world);
        var enemyFleets = GetEnemyFleets(playerId, world);
        var enemyPlanets = GetEnemyPlanets(playerId, world);

        // Determine current phase based on game state
        var currentPhase = DetermineStrategyPhase(playerId, world);
        _playerPhases[playerId] = currentPhase;

        // Clear old goals
        _goals.RemoveAll(g => g.IsExpired() || g.IsCompleted || g.IsAbandoned);

        // Add goals based on current phase
        switch (currentPhase)
        {
            case AiStrategyPhase.EarlyGame:
                AddEarlyGameGoals(playerId, world);
                break;
            case AiStrategyPhase.MidGame:
                AddMidGameGoals(playerId, world);
                break;
            case AiStrategyPhase.LateGame:
                AddLateGameGoals(playerId, world);
                break;
            case AiStrategyPhase.Defensive:
                AddDefensiveGoals(playerId, world);
                break;
            case AiStrategyPhase.Aggressive:
                AddAggressiveGoals(playerId, world);
                break;
        }
    }

    private AiStrategyPhase DetermineStrategyPhase(Guid playerId, World world)
    {
        var aiPlanets = GetPlayerPlanets(playerId, world);
        var aiFleets = GetPlayerFleets(playerId, world);
        var enemyFleets = GetEnemyFleets(playerId, world);
        var totalPlanets = world.Galaxy.StarSystems.SelectMany(s => s.Planets).Count();

        // Early game: Few planets, building up
        if (aiPlanets.Count <= 2 && aiFleets.Count <= 2)
            return AiStrategyPhase.EarlyGame;

        // Defensive: Under threat
        if (enemyFleets.Any(f =>
            {
                var location = world.Galaxy.StarSystems
                    .SelectMany(s => s.Planets)
                    .FirstOrDefault(p => p.Fleets.Contains(f));
                return location != null && aiPlanets.Any(ap =>
                    world.Galaxy.StarSystems.Any(ss => ss.Planets.Contains(ap) && ss.Planets.Contains(location)));
            }))
            return AiStrategyPhase.Defensive;

        // Aggressive: Strong position, can attack
        if (aiFleets.Count >= 3 && aiPlanets.Count >= 3)
            return AiStrategyPhase.Aggressive;

        // Late game: Most planets controlled
        if (aiPlanets.Count >= totalPlanets * 0.6)
            return AiStrategyPhase.LateGame;

        // Mid game: Default
        return AiStrategyPhase.MidGame;
    }

    private void AddEarlyGameGoals(Guid playerId, World world)
    {
        if (!_goals.Any(g => g.Type == AiGoalType.Build && !g.IsCompleted))
        {
            var buildGoal = new AiGoal(AiGoalType.Build, GoalTimeframe.ShortTerm,
                "Build economic foundation", 90.0);
            _goals.Add(buildGoal);
        }

        if (!_goals.Any(g => g.Type == AiGoalType.Expand && !g.IsCompleted))
        {
            var expandGoal = new AiGoal(AiGoalType.Expand, GoalTimeframe.ShortTerm,
                "Expand to nearby planets", 85.0);
            _goals.Add(expandGoal);
        }
    }

    private void AddMidGameGoals(Guid playerId, World world)
    {
        if (!_goals.Any(g => g.Type == AiGoalType.Build && !g.IsCompleted))
        {
            var buildGoal = new AiGoal(AiGoalType.Build, GoalTimeframe.ShortTerm,
                "Build military and economic structures", 80.0);
            _goals.Add(buildGoal);
        }

        if (!_goals.Any(g => g.Type == AiGoalType.Attack && !g.IsCompleted))
        {
            var attackGoal = new AiGoal(AiGoalType.Attack, GoalTimeframe.MediumTerm,
                "Engage enemy forces", 75.0);
            _goals.Add(attackGoal);
        }
    }

    private void AddLateGameGoals(Guid playerId, World world)
    {
        if (!_goals.Any(g => g.Type == AiGoalType.Dominate && !g.IsCompleted))
        {
            var dominateGoal = new AiGoal(AiGoalType.Dominate, GoalTimeframe.ShortTerm,
                "Dominate remaining planets", 95.0);
            _goals.Add(dominateGoal);
        }
    }

    private void AddDefensiveGoals(Guid playerId, World world)
    {
        if (!_goals.Any(g => g.Type == AiGoalType.Defend && !g.IsCompleted))
        {
            var defendGoal = new AiGoal(AiGoalType.Defend, GoalTimeframe.Immediate,
                "Defend against immediate threats", 95.0);
            _goals.Add(defendGoal);
        }

        if (!_goals.Any(g => g.Type == AiGoalType.Build && !g.IsCompleted))
        {
            var buildGoal = new AiGoal(AiGoalType.Build, GoalTimeframe.ShortTerm,
                "Build defensive structures", 85.0);
            _goals.Add(buildGoal);
        }
    }

    private void AddAggressiveGoals(Guid playerId, World world)
    {
        if (!_goals.Any(g => g.Type == AiGoalType.Attack && !g.IsCompleted))
        {
            var attackGoal = new AiGoal(AiGoalType.Attack, GoalTimeframe.ShortTerm,
                "Launch offensive operations", 90.0);
            _goals.Add(attackGoal);
        }

        if (!_goals.Any(g => g.Type == AiGoalType.Expand && !g.IsCompleted))
        {
            var expandGoal = new AiGoal(AiGoalType.Expand, GoalTimeframe.MediumTerm,
                "Expand into enemy territory", 80.0);
            _goals.Add(expandGoal);
        }
    }

    protected override List<AiDecision> GenerateDecisions(Guid playerId, World world)
    {
        var decisions = new List<AiDecision>();

        var currentPhase = _playerPhases.GetValueOrDefault(playerId, AiStrategyPhase.MidGame);
        var aiPlanets = GetPlayerPlanets(playerId, world);
        var aiFleets = GetPlayerFleets(playerId, world);
        var enemyFleets = GetEnemyFleets(playerId, world);

        // Generate decisions based on current phase
        switch (currentPhase)
        {
            case AiStrategyPhase.EarlyGame:
                decisions.AddRange(GenerateEarlyGameDecisions(playerId, aiPlanets, aiFleets, world));
                break;
            case AiStrategyPhase.MidGame:
                decisions.AddRange(GenerateMidGameDecisions(playerId, aiPlanets, aiFleets, enemyFleets, world));
                break;
            case AiStrategyPhase.LateGame:
                decisions.AddRange(GenerateLateGameDecisions(playerId, aiPlanets, aiFleets, enemyFleets, world));
                break;
            case AiStrategyPhase.Defensive:
                decisions.AddRange(GenerateDefensiveDecisions(playerId, aiPlanets, aiFleets, enemyFleets, world));
                break;
            case AiStrategyPhase.Aggressive:
                decisions.AddRange(GenerateAggressiveDecisions(playerId, aiPlanets, aiFleets, enemyFleets, world));
                break;
        }

        return decisions;
    }

    private List<AiDecision> GenerateEarlyGameDecisions(Guid playerId, List<Planet> aiPlanets, List<Fleet> aiFleets, World world)
    {
        var decisions = new List<AiDecision>();

        // Early game: Focus on building and expansion
        foreach (var planet in aiPlanets)
            if (_random.Next(100) < 50) // 50% chance to build
            {
                var earlyStructures = new[] { "Mine", "Refinery", "Construction Yard" };
                var structureType = earlyStructures[_random.Next(earlyStructures.Length)];

                var score = 70.0 + _random.NextDouble() * 20; // 70-90 score
                var decision = new AiDecision(AiDecisionType.BuildStructure, AiPriority.High, score,
                    $"Early game build: {structureType} on {planet.Name}");
                decision.AddParameter("PlanetId", planet.Id);
                decision.AddParameter("StructureType", structureType);
                decisions.Add(decision);
            }

        // Move fleets to unclaimed planets
        foreach (var fleet in aiFleets)
            if (_random.Next(100) < 40) // 40% chance to move
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
                        var score = 60.0 + _random.NextDouble() * 20; // 60-80 score
                        var decision = new AiDecision(AiDecisionType.MoveFleet, AiPriority.Medium, score,
                            $"Early expansion: move fleet {fleet.Id} to {targetPlanet.Name}");
                        decision.AddParameter("FleetId", fleet.Id);
                        decision.AddParameter("FromPlanetId", fleet.LocationPlanetId ?? Guid.Empty);
                        decision.AddParameter("ToPlanetId", targetPlanet.Id);
                        decisions.Add(decision);
                    }
                }
            }

        return decisions;
    }

    private List<AiDecision> GenerateMidGameDecisions(Guid playerId, List<Planet> aiPlanets, List<Fleet> aiFleets, List<Fleet> enemyFleets, World world)
    {
        var decisions = new List<AiDecision>();

        // Mid game: Balanced approach
        foreach (var planet in aiPlanets)
            if (_random.Next(100) < 40) // 40% chance to build
            {
                var midGameStructures = new[] { "Mine", "Refinery", "Training Facility", "Shield Generator" };
                var structureType = midGameStructures[_random.Next(midGameStructures.Length)];

                var score = 60.0 + _random.NextDouble() * 25; // 60-85 score
                var decision = new AiDecision(AiDecisionType.BuildStructure, AiPriority.Medium, score,
                    $"Mid game build: {structureType} on {planet.Name}");
                decision.AddParameter("PlanetId", planet.Id);
                decision.AddParameter("StructureType", structureType);
                decisions.Add(decision);
            }

        // Balanced combat
        if (enemyFleets.Any())
            foreach (var aiFleet in aiFleets)
                if (_random.Next(100) < 30) // 30% chance to attack
                {
                    var defender = enemyFleets[_random.Next(enemyFleets.Count)];
                    var location = world.Galaxy.StarSystems
                        .SelectMany(s => s.Planets)
                        .FirstOrDefault(p => p.Fleets.Contains(defender));

                    if (location != null)
                    {
                        var score = 65.0 + _random.NextDouble() * 25; // 65-90 score
                        var decision = new AiDecision(AiDecisionType.Attack, AiPriority.Medium, score,
                            $"Mid game attack: fleet {aiFleet.Id} attacks {defender.Id} at {location.Name}");
                        decision.AddParameter("AttackerFleetId", aiFleet.Id);
                        decision.AddParameter("DefenderFleetId", defender.Id);
                        decision.AddParameter("LocationPlanetId", location.Id);
                        decisions.Add(decision);
                    }
                }

        return decisions;
    }

    private List<AiDecision> GenerateLateGameDecisions(Guid playerId, List<Planet> aiPlanets, List<Fleet> aiFleets, List<Fleet> enemyFleets, World world)
    {
        var decisions = new List<AiDecision>();

        // Late game: Aggressive domination
        if (enemyFleets.Any())
            foreach (var aiFleet in aiFleets)
                if (_random.Next(100) < 60) // 60% chance to attack
                {
                    var defender = enemyFleets[_random.Next(enemyFleets.Count)];
                    var location = world.Galaxy.StarSystems
                        .SelectMany(s => s.Planets)
                        .FirstOrDefault(p => p.Fleets.Contains(defender));

                    if (location != null)
                    {
                        var score = 80.0 + _random.NextDouble() * 20; // 80-100 score
                        var decision = new AiDecision(AiDecisionType.Attack, AiPriority.High, score,
                            $"Late game domination: fleet {aiFleet.Id} attacks {defender.Id} at {location.Name}");
                        decision.AddParameter("AttackerFleetId", aiFleet.Id);
                        decision.AddParameter("DefenderFleetId", defender.Id);
                        decision.AddParameter("LocationPlanetId", location.Id);
                        decisions.Add(decision);
                    }
                }

        return decisions;
    }

    private List<AiDecision> GenerateDefensiveDecisions(Guid playerId, List<Planet> aiPlanets, List<Fleet> aiFleets, List<Fleet> enemyFleets, World world)
    {
        var decisions = new List<AiDecision>();

        // Defensive: Build defensive structures
        foreach (var planet in aiPlanets)
            if (_random.Next(100) < 60) // 60% chance to build
            {
                var defensiveStructures = new[] { "Shield Generator", "Training Facility" };
                var structureType = defensiveStructures[_random.Next(defensiveStructures.Length)];

                var score = 75.0 + _random.NextDouble() * 20; // 75-95 score
                var decision = new AiDecision(AiDecisionType.BuildStructure, AiPriority.High, score,
                    $"Defensive build: {structureType} on {planet.Name}");
                decision.AddParameter("PlanetId", planet.Id);
                decision.AddParameter("StructureType", structureType);
                decisions.Add(decision);
            }

        // Defensive: Attack only nearby threats
        if (enemyFleets.Any())
            foreach (var aiFleet in aiFleets)
                if (_random.Next(100) < 35) // 35% chance to attack
                {
                    var playerPlanets = GetPlayerPlanets(playerId, world);
                    var nearbyEnemies = enemyFleets.Where(f =>
                    {
                        var location = world.Galaxy.StarSystems
                            .SelectMany(s => s.Planets)
                            .FirstOrDefault(p => p.Fleets.Contains(f));
                        return location != null && playerPlanets.Any(ap =>
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
                            var score = 70.0 + _random.NextDouble() * 20; // 70-90 score
                            var decision = new AiDecision(AiDecisionType.Attack, AiPriority.Medium, score,
                                $"Defensive attack: eliminate threat {defender.Id} at {location.Name}");
                            decision.AddParameter("AttackerFleetId", aiFleet.Id);
                            decision.AddParameter("DefenderFleetId", defender.Id);
                            decision.AddParameter("LocationPlanetId", location.Id);
                            decisions.Add(decision);
                        }
                    }
                }

        return decisions;
    }

    private List<AiDecision> GenerateAggressiveDecisions(Guid playerId, List<Planet> aiPlanets, List<Fleet> aiFleets, List<Fleet> enemyFleets, World world)
    {
        var decisions = new List<AiDecision>();

        // Aggressive: Build military structures
        foreach (var planet in aiPlanets)
            if (_random.Next(100) < 45) // 45% chance to build
            {
                var militaryStructures = new[] { "Training Facility", "Shield Generator", "Shipyard" };
                var structureType = militaryStructures[_random.Next(militaryStructures.Length)];

                var score = 65.0 + _random.NextDouble() * 25; // 65-90 score
                var decision = new AiDecision(AiDecisionType.BuildStructure, AiPriority.Medium, score,
                    $"Aggressive build: {structureType} on {planet.Name}");
                decision.AddParameter("PlanetId", planet.Id);
                decision.AddParameter("StructureType", structureType);
                decisions.Add(decision);
            }

        // Aggressive: Attack frequently
        if (enemyFleets.Any())
            foreach (var aiFleet in aiFleets)
                if (_random.Next(100) < 50) // 50% chance to attack
                {
                    var defender = enemyFleets[_random.Next(enemyFleets.Count)];
                    var location = world.Galaxy.StarSystems
                        .SelectMany(s => s.Planets)
                        .FirstOrDefault(p => p.Fleets.Contains(defender));

                    if (location != null)
                    {
                        var score = 75.0 + _random.NextDouble() * 20; // 75-95 score
                        var decision = new AiDecision(AiDecisionType.Attack, AiPriority.High, score,
                            $"Aggressive attack: fleet {aiFleet.Id} attacks {defender.Id} at {location.Name}");
                        decision.AddParameter("AttackerFleetId", aiFleet.Id);
                        decision.AddParameter("DefenderFleetId", defender.Id);
                        decision.AddParameter("LocationPlanetId", location.Id);
                        decisions.Add(decision);
                    }
                }

        return decisions;
    }

    protected override AiDecision? EvaluateFleetMovement(Guid playerId, Fleet fleet, World world)
    {
        var currentPhase = _playerPhases.GetValueOrDefault(playerId, AiStrategyPhase.MidGame);

        // Adjust movement based on phase
        var moveChance = currentPhase switch
        {
            AiStrategyPhase.EarlyGame => 40,
            AiStrategyPhase.MidGame => 30,
            AiStrategyPhase.LateGame => 50,
            AiStrategyPhase.Defensive => 35,
            AiStrategyPhase.Aggressive => 45,
            _ => 30
        };

        if (_random.Next(100) < moveChance)
        {
            var targetPlanet = currentPhase switch
            {
                AiStrategyPhase.EarlyGame => GetUnclaimedPlanet(world),
                AiStrategyPhase.Defensive => GetDefensivePlanet(playerId, world),
                AiStrategyPhase.Aggressive => GetEnemyPlanet(playerId, world),
                _ => GetRandomPlanet(world)
            };

            if (fleet.LocationPlanetId != targetPlanet.Id)
            {
                var score = 55.0 + _random.NextDouble() * 30; // 55-85 score
                var decision = new AiDecision(AiDecisionType.MoveFleet, AiPriority.Medium, score,
                    $"Balanced move ({currentPhase}): fleet {fleet.Id} to {targetPlanet.Name}");
                decision.AddParameter("FleetId", fleet.Id);
                decision.AddParameter("FromPlanetId", fleet.LocationPlanetId ?? Guid.Empty);
                decision.AddParameter("ToPlanetId", targetPlanet.Id);
                return decision;
            }
        }

        return null;
    }

    private Planet GetUnclaimedPlanet(World world)
    {
        var unclaimedPlanets = world.Galaxy.StarSystems
            .SelectMany(s => s.Planets)
            .Where(p => !p.Fleets.Any())
            .ToList();

        return unclaimedPlanets.Any()
            ? unclaimedPlanets[_random.Next(unclaimedPlanets.Count)]
            : GetRandomPlanet(world);
    }

    private Planet GetDefensivePlanet(Guid playerId, World world)
    {
        var aiPlanets = GetPlayerPlanets(playerId, world);
        return aiPlanets.Any()
            ? aiPlanets[_random.Next(aiPlanets.Count)]
            : GetRandomPlanet(world);
    }

    private Planet GetEnemyPlanet(Guid playerId, World world)
    {
        var enemyPlanets = GetEnemyPlanets(playerId, world);
        return enemyPlanets.Any()
            ? enemyPlanets[_random.Next(enemyPlanets.Count)]
            : GetRandomPlanet(world);
    }

    private Planet GetRandomPlanet(World world)
    {
        var allPlanets = world.Galaxy.StarSystems.SelectMany(s => s.Planets).ToList();
        return allPlanets[_random.Next(allPlanets.Count)];
    }

    public void SetBalanceLevel(double level)
    {
        // Adjust balance level (0.0 to 1.0)
        // Higher levels improve decision-making across all phases
        Console.WriteLine($"Setting BalancedAiStrategy balance level to {level}");
    }
}
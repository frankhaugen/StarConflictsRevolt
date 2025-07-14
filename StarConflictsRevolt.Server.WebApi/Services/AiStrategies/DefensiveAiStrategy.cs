using StarConflictsRevolt.Server.WebApi.Eventing;
using StarConflictsRevolt.Server.WebApi.Models;

namespace StarConflictsRevolt.Server.WebApi.Services.AiStrategies;

public class DefensiveAiStrategy : BaseAiStrategy
{
    public DefensiveAiStrategy(AiMemoryBank memoryBank) : base(memoryBank)
    {
    }

    public override List<IGameEvent> GenerateCommands(Guid playerId, World world, ILogger logger)
    {
        if (!CanAct(playerId, TimeSpan.FromSeconds(3))) // Defensive AI acts moderately
            return new List<IGameEvent>();

        RecordAction(playerId);
        return GenerateCommandsInternal(playerId, world, logger);
    }

    protected override void UpdateGoals(Guid playerId, World world)
    {
        var aiPlanets = GetPlayerPlanets(playerId, world);
        var aiFleets = GetPlayerFleets(playerId, world);
        var enemyFleets = GetEnemyFleets(playerId, world);

        // Defensive goals
        if (!_goals.Any(g => g.Type == AiGoalType.Defend && !g.IsCompleted))
        {
            var defendGoal = new AiGoal(AiGoalType.Defend, GoalTimeframe.Immediate, 
                "Fortify planets and protect assets", 95.0);
            _goals.Add(defendGoal);
        }

        if (aiPlanets.Any(p => p.Structures.Count < 2) && !_goals.Any(g => g.Type == AiGoalType.Build && !g.IsCompleted))
        {
            var buildGoal = new AiGoal(AiGoalType.Build, GoalTimeframe.ShortTerm, 
                "Build defensive structures", 90.0);
            _goals.Add(buildGoal);
        }

        if (enemyFleets.Any() && !_goals.Any(g => g.Type == AiGoalType.Attack && !g.IsCompleted))
        {
            var attackGoal = new AiGoal(AiGoalType.Attack, GoalTimeframe.Immediate, 
                "Eliminate threats to our planets", 85.0);
            _goals.Add(attackGoal);
        }
    }

    protected override List<AiDecision> GenerateDecisions(Guid playerId, World world)
    {
        var decisions = new List<AiDecision>();
        
        var aiPlanets = GetPlayerPlanets(playerId, world);
        var aiFleets = GetPlayerFleets(playerId, world);
        var enemyFleets = GetEnemyFleets(playerId, world);

        // Defensive AI prioritizes protection and fortification
        decisions.AddRange(GenerateDefensiveBuildingDecisions(playerId, aiPlanets, world));
        decisions.AddRange(GenerateThreatEliminationDecisions(playerId, aiFleets, enemyFleets, world));
        decisions.AddRange(GenerateDefensivePositioningDecisions(playerId, aiFleets, aiPlanets, world));
        decisions.AddRange(GenerateFleetDecisions(playerId, aiFleets, world));

        return decisions;
    }

    private List<AiDecision> GenerateDefensiveBuildingDecisions(Guid playerId, List<Planet> aiPlanets, World world)
    {
        var decisions = new List<AiDecision>();

        foreach (var planet in aiPlanets)
        {
            // Defensive AI builds defensive structures
            if (_random.Next(100) < 70) // 70% chance to build (very high for defensive)
            {
                var defensiveStructures = new[] { "Shield Generator", "Training Facility", "Shield Generator" };
                var structureType = defensiveStructures[_random.Next(defensiveStructures.Length)];
                
                var score = 85.0 + _random.NextDouble() * 15; // 85-100 score (very high priority)
                var decision = new AiDecision(AiDecisionType.BuildStructure, AiPriority.Critical, score,
                    $"Build {structureType} on {planet.Name} for defense");
                decision.AddParameter("PlanetId", planet.Id);
                decision.AddParameter("StructureType", structureType);
                decisions.Add(decision);
            }
        }

        return decisions;
    }

    private List<AiDecision> GenerateThreatEliminationDecisions(Guid playerId, List<Fleet> aiFleets, List<Fleet> enemyFleets, World world)
    {
        var decisions = new List<AiDecision>();

        if (!aiFleets.Any() || !enemyFleets.Any())
            return decisions;

        // Defensive AI attacks only when threatened
        foreach (var aiFleet in aiFleets)
        {
            if (_random.Next(100) < 40) // 40% chance to attack (moderate for defensive)
            {
                // Find enemies near our planets
                var aiPlanets = GetPlayerPlanets(playerId, world);
                var threateningEnemies = enemyFleets.Where(f => 
                {
                    var location = world.Galaxy.StarSystems
                        .SelectMany(s => s.Planets)
                        .FirstOrDefault(p => p.Fleets.Contains(f));
                    return location != null && aiPlanets.Any(ap => 
                        world.Galaxy.StarSystems.Any(ss => ss.Planets.Contains(ap) && ss.Planets.Contains(location)));
                }).ToList();

                if (threateningEnemies.Any())
                {
                    var defender = threateningEnemies[_random.Next(threateningEnemies.Count)];
                    var location = world.Galaxy.StarSystems
                        .SelectMany(s => s.Planets)
                        .FirstOrDefault(p => p.Fleets.Contains(defender));

                    if (location != null)
                    {
                        var score = 75.0 + _random.NextDouble() * 20; // 75-95 score (high priority)
                        var decision = new AiDecision(AiDecisionType.Attack, AiPriority.High, score,
                            $"Eliminate threat: attack enemy fleet {defender.Id} near our territory at {location.Name}");
                        decision.AddParameter("AttackerFleetId", aiFleet.Id);
                        decision.AddParameter("DefenderFleetId", defender.Id);
                        decision.AddParameter("LocationPlanetId", location.Id);
                        decisions.Add(decision);
                    }
                }
            }
        }

        return decisions;
    }

    private List<AiDecision> GenerateDefensivePositioningDecisions(Guid playerId, List<Fleet> aiFleets, List<Planet> aiPlanets, World world)
    {
        var decisions = new List<AiDecision>();

        if (!aiFleets.Any() || !aiPlanets.Any())
            return decisions;

        // Defensive AI positions fleets to protect planets
        foreach (var fleet in aiFleets)
        {
            if (_random.Next(100) < 45) // 45% chance to move for defense
            {
                // Move to protect our most vulnerable planet
                var vulnerablePlanets = aiPlanets.Where(p => p.Structures.Count < 2).ToList();
                var targetPlanet = vulnerablePlanets.Any() 
                    ? vulnerablePlanets[_random.Next(vulnerablePlanets.Count)]
                    : aiPlanets[_random.Next(aiPlanets.Count)];
                
                if (fleet.LocationPlanetId != targetPlanet.Id)
                {
                    var score = 70.0 + _random.NextDouble() * 20; // 70-90 score
                    var decision = new AiDecision(AiDecisionType.MoveFleet, AiPriority.High, score,
                        $"Position fleet {fleet.Id} to defend {targetPlanet.Name}");
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
        // Defensive AI moves to protect planets
        if (_random.Next(100) < 35) // 35% chance (moderate)
        {
            var aiPlanets = GetPlayerPlanets(playerId, world);
            var targetPlanet = aiPlanets.Any() 
                ? aiPlanets[_random.Next(aiPlanets.Count)]
                : world.Galaxy.StarSystems.SelectMany(s => s.Planets).ToList()[_random.Next(world.Galaxy.StarSystems.SelectMany(s => s.Planets).Count())];
            
            if (fleet.LocationPlanetId != targetPlanet.Id)
            {
                var score = 60.0 + _random.NextDouble() * 25; // 60-85 score
                var decision = new AiDecision(AiDecisionType.MoveFleet, AiPriority.Medium, score, 
                    $"Defensive move: fleet {fleet.Id} to protect {targetPlanet.Name}");
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
        // Defensive AI builds defensive structures frequently
        if (_random.Next(100) < 60) // 60% chance (high for defensive)
        {
            var defensiveStructures = new[] { "Shield Generator", "Training Facility", "Shield Generator" };
            var structureType = defensiveStructures[_random.Next(defensiveStructures.Length)];
            
            var score = 75.0 + _random.NextDouble() * 20; // 75-95 score (high priority)
            var decision = new AiDecision(AiDecisionType.BuildStructure, AiPriority.High, score,
                $"Defensive build: {structureType} on {planet.Name}");
            decision.AddParameter("PlanetId", planet.Id);
            decision.AddParameter("StructureType", structureType);
            return decision;
        }
        
        return null;
    }

    protected override AiDecision? EvaluateCombatOpportunity(Guid playerId, Fleet aiFleet, List<Fleet> enemyFleets, World world)
    {
        // Defensive AI attacks only when threatened
        if (_random.Next(100) < 20) // 20% chance (low for defensive)
        {
            // Only attack if enemy is near our planets
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
                    var score = 65.0 + _random.NextDouble() * 25; // 65-90 score (moderate priority)
                    var decision = new AiDecision(AiDecisionType.Attack, AiPriority.Medium, score,
                        $"Defensive attack: eliminate threat {defender.Id} at {location.Name}");
                    decision.AddParameter("AttackerFleetId", aiFleet.Id);
                    decision.AddParameter("DefenderFleetId", defender.Id);
                    decision.AddParameter("LocationPlanetId", location.Id);
                    return decision;
                }
            }
        }
        
        return null;
    }
} 
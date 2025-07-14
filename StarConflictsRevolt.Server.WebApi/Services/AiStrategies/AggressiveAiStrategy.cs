using StarConflictsRevolt.Server.WebApi.Eventing;
using StarConflictsRevolt.Server.WebApi.Models;

namespace StarConflictsRevolt.Server.WebApi.Services.AiStrategies;

public class AggressiveAiStrategy : BaseAiStrategy
{
    public AggressiveAiStrategy(AiMemoryBank memoryBank) : base(memoryBank)
    {
    }

    public override List<IGameEvent> GenerateCommands(Guid playerId, World world, ILogger logger)
    {
        if (!CanAct(playerId, TimeSpan.FromSeconds(2))) // Aggressive AI acts quickly
            return new List<IGameEvent>();

        RecordAction(playerId);
        return GenerateCommandsInternal(playerId, world, logger);
    }

    protected override void UpdateGoals(Guid playerId, World world)
    {
        var aiFleets = GetPlayerFleets(playerId, world);
        var aiPlanets = GetPlayerPlanets(playerId, world);
        var enemyFleets = GetEnemyFleets(playerId, world);
        var enemyPlanets = GetEnemyPlanets(playerId, world);

        // Aggressive goals
        if (enemyFleets.Any() && !_goals.Any(g => g.Type == AiGoalType.Attack && !g.IsCompleted))
        {
            var attackGoal = new AiGoal(AiGoalType.Attack, GoalTimeframe.ShortTerm, 
                "Attack enemy fleets", 90.0);
            _goals.Add(attackGoal);
        }

        if (enemyPlanets.Any() && !_goals.Any(g => g.Type == AiGoalType.Expand && !g.IsCompleted))
        {
            var expandGoal = new AiGoal(AiGoalType.Expand, GoalTimeframe.MediumTerm, 
                "Expand to enemy planets", 85.0);
            _goals.Add(expandGoal);
        }

        if (aiFleets.Count < 3 && !_goals.Any(g => g.Type == AiGoalType.Build && !g.IsCompleted))
        {
            var buildGoal = new AiGoal(AiGoalType.Build, GoalTimeframe.ShortTerm, 
                "Build more fleets", 75.0);
            _goals.Add(buildGoal);
        }
    }

    protected override List<AiDecision> GenerateDecisions(Guid playerId, World world)
    {
        var decisions = new List<AiDecision>();
        
        var aiFleets = GetPlayerFleets(playerId, world);
        var aiPlanets = GetPlayerPlanets(playerId, world);
        var enemyFleets = GetEnemyFleets(playerId, world);
        var enemyPlanets = GetEnemyPlanets(playerId, world);

        // Aggressive AI prioritizes combat and expansion
        decisions.AddRange(GenerateAggressiveCombatDecisions(playerId, aiFleets, enemyFleets, world));
        decisions.AddRange(GenerateExpansionDecisions(playerId, aiFleets, enemyPlanets, world));
        decisions.AddRange(GenerateFleetBuildingDecisions(playerId, aiPlanets, world));
        decisions.AddRange(GenerateFleetDecisions(playerId, aiFleets, world));

        return decisions;
    }

    private List<AiDecision> GenerateAggressiveCombatDecisions(Guid playerId, List<Fleet> aiFleets, List<Fleet> enemyFleets, World world)
    {
        var decisions = new List<AiDecision>();

        if (!aiFleets.Any() || !enemyFleets.Any())
            return decisions;

        // Aggressive AI has high chance to attack
        foreach (var aiFleet in aiFleets)
        {
            if (_random.Next(100) < 60) // 60% chance to attack (high for aggressive)
            {
                // Find the weakest enemy fleet
                var weakestEnemy = enemyFleets.OrderBy(f => f.Ships.Count).First();
                var location = world.Galaxy.StarSystems
                    .SelectMany(s => s.Planets)
                    .FirstOrDefault(p => p.Fleets.Contains(weakestEnemy));

                if (location != null)
                {
                    var score = 85.0 + _random.NextDouble() * 15; // 85-100 score (high priority)
                    var decision = new AiDecision(AiDecisionType.Attack, AiPriority.Critical, score,
                        $"Aggressive attack on enemy fleet {weakestEnemy.Id} at {location.Name}");
                    decision.AddParameter("AttackerFleetId", aiFleet.Id);
                    decision.AddParameter("DefenderFleetId", weakestEnemy.Id);
                    decision.AddParameter("LocationPlanetId", location.Id);
                    decisions.Add(decision);
                }
            }
        }

        return decisions;
    }

    private List<AiDecision> GenerateExpansionDecisions(Guid playerId, List<Fleet> aiFleets, List<Planet> enemyPlanets, World world)
    {
        var decisions = new List<AiDecision>();

        if (!aiFleets.Any() || !enemyPlanets.Any())
            return decisions;

        // Aggressive AI moves fleets toward enemy planets
        foreach (var fleet in aiFleets)
        {
            if (_random.Next(100) < 50) // 50% chance to move toward enemy
            {
                var targetPlanet = enemyPlanets[_random.Next(enemyPlanets.Count)];
                
                if (fleet.LocationPlanetId != targetPlanet.Id)
                {
                    var score = 70.0 + _random.NextDouble() * 20; // 70-90 score
                    var decision = new AiDecision(AiDecisionType.MoveFleet, AiPriority.High, score,
                        $"Move fleet {fleet.Id} toward enemy planet {targetPlanet.Name}");
                    decision.AddParameter("FleetId", fleet.Id);
                    decision.AddParameter("FromPlanetId", fleet.LocationPlanetId ?? Guid.Empty);
                    decision.AddParameter("ToPlanetId", targetPlanet.Id);
                    decisions.Add(decision);
                }
            }
        }

        return decisions;
    }

    private List<AiDecision> GenerateFleetBuildingDecisions(Guid playerId, List<Planet> aiPlanets, World world)
    {
        var decisions = new List<AiDecision>();

        foreach (var planet in aiPlanets)
        {
            // Aggressive AI builds military structures
            if (_random.Next(100) < 40) // 40% chance to build
            {
                var militaryStructures = new[] { "Training Facility", "Shield Generator" };
                var structureType = militaryStructures[_random.Next(militaryStructures.Length)];
                
                var score = 60.0 + _random.NextDouble() * 20; // 60-80 score
                var decision = new AiDecision(AiDecisionType.BuildStructure, AiPriority.Medium, score,
                    $"Build {structureType} on {planet.Name} for military advantage");
                decision.AddParameter("PlanetId", planet.Id);
                decision.AddParameter("StructureType", structureType);
                decisions.Add(decision);
            }
        }

        return decisions;
    }

    protected override AiDecision? EvaluateFleetMovement(Guid playerId, Fleet fleet, World world)
    {
        // Aggressive AI moves more frequently and toward enemies
        if (_random.Next(100) < 50) // 50% chance (higher than base)
        {
            var enemyPlanets = GetEnemyPlanets(playerId, world);
            var targetPlanet = enemyPlanets.Any() 
                ? enemyPlanets[_random.Next(enemyPlanets.Count)]
                : world.Galaxy.StarSystems.SelectMany(s => s.Planets).ToList()[_random.Next(world.Galaxy.StarSystems.SelectMany(s => s.Planets).Count())];
            
            if (fleet.LocationPlanetId != targetPlanet.Id)
            {
                var score = 65.0 + _random.NextDouble() * 25; // 65-90 score
                var decision = new AiDecision(AiDecisionType.MoveFleet, AiPriority.High, score, 
                    $"Aggressive move: fleet {fleet.Id} to {targetPlanet.Name}");
                decision.AddParameter("FleetId", fleet.Id);
                decision.AddParameter("FromPlanetId", fleet.LocationPlanetId ?? Guid.Empty);
                decision.AddParameter("ToPlanetId", targetPlanet.Id);
                return decision;
            }
        }
        
        return null;
    }

    protected override AiDecision? EvaluateCombatOpportunity(Guid playerId, Fleet aiFleet, List<Fleet> enemyFleets, World world)
    {
        // Aggressive AI attacks more frequently
        if (_random.Next(100) < 45) // 45% chance (higher than base)
        {
            var defender = enemyFleets[_random.Next(enemyFleets.Count)];
            var location = world.Galaxy.StarSystems
                .SelectMany(s => s.Planets)
                .FirstOrDefault(p => p.Fleets.Contains(defender));
                
            if (location != null)
            {
                var score = 80.0 + _random.NextDouble() * 20; // 80-100 score (high priority)
                var decision = new AiDecision(AiDecisionType.Attack, AiPriority.Critical, score,
                    $"Aggressive attack: fleet {aiFleet.Id} attacks {defender.Id} at {location.Name}");
                decision.AddParameter("AttackerFleetId", aiFleet.Id);
                decision.AddParameter("DefenderFleetId", defender.Id);
                decision.AddParameter("LocationPlanetId", location.Id);
                return decision;
            }
        }
        
        return null;
    }
} 
using StarConflictsRevolt.Server.WebApi.Enums;
using StarConflictsRevolt.Server.WebApi.Eventing;
using StarConflictsRevolt.Server.WebApi.Models;

namespace StarConflictsRevolt.Server.WebApi.Services.AiStrategies;

public abstract class BaseAiStrategy : IAiStrategy
{
    protected readonly Random _random = new();
    protected readonly AiMemoryBank _memoryBank;
    protected readonly List<AiGoal> _goals = new();
    protected readonly Dictionary<Guid, DateTime> _lastActionTime = new();

    protected BaseAiStrategy(AiMemoryBank memoryBank)
    {
        _memoryBank = memoryBank;
    }

    public abstract List<IGameEvent> GenerateCommands(Guid playerId, World world, ILogger logger);

    protected virtual List<IGameEvent> GenerateCommandsInternal(Guid playerId, World world, ILogger logger)
    {
        var commands = new List<IGameEvent>();
        
        // Update goals and remove expired ones
        UpdateGoals(playerId, world);
        CleanupExpiredGoals();
        
        // Generate decisions based on current goals
        var decisions = GenerateDecisions(playerId, world);
        
        // Convert decisions to game events
        foreach (var decision in decisions.OrderByDescending(d => d.Score).Take(5)) // Limit to top 5 decisions
        {
            var gameEvent = ConvertDecisionToGameEvent(decision, playerId, world);
            if (gameEvent != null)
            {
                commands.Add(gameEvent);
                decision.MarkExecuted();
                
                // Remember this decision
                var memory = new AiMemory(playerId, MemoryType.Decision, decision.Description, decision.Score / 100.0);
                memory.AddData("DecisionType", decision.Type);
                memory.AddData("Priority", decision.Priority);
                memory.AddData("Parameters", decision.Parameters);
                _memoryBank.AddMemory(memory);
            }
        }

        return commands;
    }

    protected virtual void UpdateGoals(Guid playerId, World world)
    {
        // Override in derived classes to implement specific goal logic
    }

    protected virtual void CleanupExpiredGoals()
    {
        _goals.RemoveAll(g => g.IsExpired() || g.IsCompleted || g.IsAbandoned);
    }

    protected virtual List<AiDecision> GenerateDecisions(Guid playerId, World world)
    {
        var decisions = new List<AiDecision>();
        
        // Get AI's assets
        var aiFleets = GetPlayerFleets(playerId, world);
        var aiPlanets = GetPlayerPlanets(playerId, world);
        var enemyFleets = GetEnemyFleets(playerId, world);
        var enemyPlanets = GetEnemyPlanets(playerId, world);

        // Generate decisions based on current situation
        decisions.AddRange(GenerateFleetDecisions(playerId, aiFleets, world));
        decisions.AddRange(GenerateBuildingDecisions(playerId, aiPlanets, world));
        decisions.AddRange(GenerateCombatDecisions(playerId, aiFleets, enemyFleets, world));

        return decisions;
    }

    protected virtual List<AiDecision> GenerateFleetDecisions(Guid playerId, List<Fleet> aiFleets, World world)
    {
        var decisions = new List<AiDecision>();
        
        foreach (var fleet in aiFleets)
        {
            // Check if fleet should move
            var moveDecision = EvaluateFleetMovement(playerId, fleet, world);
            if (moveDecision != null)
            {
                decisions.Add(moveDecision);
            }
        }

        return decisions;
    }

    protected virtual List<AiDecision> GenerateBuildingDecisions(Guid playerId, List<Planet> aiPlanets, World world)
    {
        var decisions = new List<AiDecision>();
        
        foreach (var planet in aiPlanets)
        {
            // Check if planet should build something
            var buildDecision = EvaluateBuilding(playerId, planet, world);
            if (buildDecision != null)
            {
                decisions.Add(buildDecision);
            }
        }

        return decisions;
    }

    protected virtual List<AiDecision> GenerateCombatDecisions(Guid playerId, List<Fleet> aiFleets, List<Fleet> enemyFleets, World world)
    {
        var decisions = new List<AiDecision>();
        
        if (!aiFleets.Any() || !enemyFleets.Any())
            return decisions;

        // Find combat opportunities
        foreach (var aiFleet in aiFleets)
        {
            var combatDecision = EvaluateCombatOpportunity(playerId, aiFleet, enemyFleets, world);
            if (combatDecision != null)
            {
                decisions.Add(combatDecision);
            }
        }

        return decisions;
    }

    protected virtual AiDecision? EvaluateFleetMovement(Guid playerId, Fleet fleet, World world)
    {
        // Base implementation - can be overridden by specific strategies
        if (_random.Next(100) < 30) // 30% chance to move
        {
            var allPlanets = world.Galaxy.StarSystems.SelectMany(s => s.Planets).ToList();
            var targetPlanet = allPlanets[_random.Next(allPlanets.Count)];
            
            if (fleet.LocationPlanetId != targetPlanet.Id)
            {
                var score = _random.NextDouble() * 50 + 25; // 25-75 score
                var decision = new AiDecision(AiDecisionType.MoveFleet, AiPriority.Medium, score, 
                    $"Move fleet {fleet.Id} to {targetPlanet.Name}");
                decision.AddParameter("FleetId", fleet.Id);
                decision.AddParameter("FromPlanetId", fleet.LocationPlanetId ?? Guid.Empty);
                decision.AddParameter("ToPlanetId", targetPlanet.Id);
                return decision;
            }
        }
        
        return null;
    }

    protected virtual AiDecision? EvaluateBuilding(Guid playerId, Planet planet, World world)
    {
        // Base implementation - can be overridden by specific strategies
        if (_random.Next(100) < 20) // 20% chance to build
        {
            var structureTypes = Enum.GetValues<StructureVariant>();
            var structureType = structureTypes[_random.Next(structureTypes.Length)];
            
            var score = _random.NextDouble() * 40 + 30; // 30-70 score
            var decision = new AiDecision(AiDecisionType.BuildStructure, AiPriority.Medium, score,
                $"Build {structureType} on {planet.Name}");
            decision.AddParameter("PlanetId", planet.Id);
            decision.AddParameter("StructureType", structureType.ToString());
            return decision;
        }
        
        return null;
    }

    protected virtual AiDecision? EvaluateCombatOpportunity(Guid playerId, Fleet aiFleet, List<Fleet> enemyFleets, World world)
    {
        // Base implementation - can be overridden by specific strategies
        if (_random.Next(100) < 15) // 15% chance to attack
        {
            var defender = enemyFleets[_random.Next(enemyFleets.Count)];
            var location = world.Galaxy.StarSystems
                .SelectMany(s => s.Planets)
                .FirstOrDefault(p => p.Fleets.Contains(defender));
                
            if (location != null)
            {
                var score = _random.NextDouble() * 60 + 40; // 40-100 score
                var decision = new AiDecision(AiDecisionType.Attack, AiPriority.High, score,
                    $"Attack enemy fleet {defender.Id} at {location.Name}");
                decision.AddParameter("AttackerFleetId", aiFleet.Id);
                decision.AddParameter("DefenderFleetId", defender.Id);
                decision.AddParameter("LocationPlanetId", location.Id);
                return decision;
            }
        }
        
        return null;
    }

    protected virtual IGameEvent? ConvertDecisionToGameEvent(AiDecision decision, Guid playerId, World world)
    {
        return decision.Type switch
        {
            AiDecisionType.MoveFleet => new MoveFleetEvent(
                playerId,
                decision.GetParameter<Guid>("FleetId"),
                decision.GetParameter<Guid>("FromPlanetId"),
                decision.GetParameter<Guid>("ToPlanetId")),
                
            AiDecisionType.BuildStructure => new BuildStructureEvent(
                playerId,
                decision.GetParameter<Guid>("PlanetId"),
                decision.GetParameter<string>("StructureType") ?? "Mine"),
                
            AiDecisionType.Attack => new AttackEvent(
                playerId,
                decision.GetParameter<Guid>("AttackerFleetId"),
                decision.GetParameter<Guid>("DefenderFleetId"),
                decision.GetParameter<Guid>("LocationPlanetId")),
                
            _ => null
        };
    }

    protected List<Fleet> GetPlayerFleets(Guid playerId, World world)
    {
        return world.Galaxy.StarSystems
            .SelectMany(s => s.Planets)
            .SelectMany(p => p.Fleets)
            .Where(f => f.Id == playerId)
            .ToList();
    }

    protected List<Planet> GetPlayerPlanets(Guid playerId, World world)
    {
        return world.Galaxy.StarSystems
            .SelectMany(s => s.Planets)
            .Where(p => p.Fleets.Any(f => f.Id == playerId))
            .ToList();
    }

    protected List<Fleet> GetEnemyFleets(Guid playerId, World world)
    {
        return world.Galaxy.StarSystems
            .SelectMany(s => s.Planets)
            .SelectMany(p => p.Fleets)
            .Where(f => f.Id != playerId)
            .ToList();
    }

    protected List<Planet> GetEnemyPlanets(Guid playerId, World world)
    {
        return world.Galaxy.StarSystems
            .SelectMany(s => s.Planets)
            .Where(p => p.Fleets.Any(f => f.Id != playerId))
            .ToList();
    }

    protected bool CanAct(Guid playerId, TimeSpan cooldown)
    {
        if (!_lastActionTime.TryGetValue(playerId, out var lastAction))
            return true;

        return DateTime.UtcNow - lastAction >= cooldown;
    }

    protected void RecordAction(Guid playerId)
    {
        _lastActionTime[playerId] = DateTime.UtcNow;
    }
} 
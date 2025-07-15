using StarConflictsRevolt.Server.WebApi.Core.Domain.Combat;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Planets;

namespace StarConflictsRevolt.Server.WebApi.Application.Services.Combat;

public class MissionSimulator : IMissionSimulator
{
    private readonly ILogger<MissionSimulator> _logger;

    public MissionSimulator(ILogger<MissionSimulator> logger)
    {
        _logger = logger;
    }

    public CombatResult SimulateMission(Mission mission, Character agent, Planet target)
    {
        _logger.LogInformation("Starting mission simulation. Mission: {MissionId}, Agent: {AgentId}, Target: {TargetId}",
            mission.Id, agent.Id, target.Id);

        // Check if character can complete the mission
        if (!CanCharacterCompleteMission(mission, agent))
        {
            _logger.LogWarning("Character {CharacterId} cannot complete mission {MissionId} - requirements not met", 
                agent.Id, mission.Id);
            
            return new CombatResult
            {
                CombatId = Guid.NewGuid(),
                Type = CombatType.Mission,
                AttackerVictory = false,
                RoundsFought = 1,
                Duration = TimeSpan.FromMinutes(5),
                AttackerLosses = new List<CombatShip>(),
                DefenderLosses = new List<CombatShip>()
            };
        }

        // Calculate success chance
        var successChance = CalculateSuccessChance(mission.BaseDifficulty, CalculateSkillBonus(agent, mission.Type), CalculateEnvironmentalModifier(target));
        var random = Random.Shared.NextDouble();

        // Determine mission outcome
        MissionOutcome outcome;
        if (random < 0.05) // 5% critical failure chance
        {
            outcome = MissionOutcome.CriticalFailure;
        }
        else if (random < 0.10) // 5% critical success chance
        {
            outcome = MissionOutcome.CriticalSuccess;
        }
        else if (random < successChance)
        {
            outcome = MissionOutcome.Success;
        }
        else
        {
            outcome = MissionOutcome.Failure;
        }

        // Apply mission results
        var result = ApplyMissionResults(mission, agent, target, outcome);

        _logger.LogInformation("Mission simulation completed. Outcome: {Outcome}, Success: {Success}", 
            outcome, result.AttackerVictory);

        return result;
    }

    public double CalculateMissionDifficulty(Mission mission, Character agent, Planet target)
    {
        var baseDifficulty = 50.0;
        
        // Adjust based on mission type
        baseDifficulty += mission.Type switch
        {
            MissionType.Assassination => 30,
            MissionType.Sabotage => 25,
            MissionType.Espionage => 20,
            MissionType.Diplomacy => 15,
            MissionType.Training => 10,
            _ => 0
        };
        
        // Adjust based on target planet
        if (target.Structures.Count > 5) baseDifficulty += 10;
        if (target.Fleets.Count > 0) baseDifficulty += 15;
        
        // Adjust based on character level
        baseDifficulty -= (agent.Level - 1) * 2;
        
        return Math.Clamp(baseDifficulty, 10.0, 100.0);
    }

    public double CalculateSkillBonus(Character agent, MissionType missionType)
    {
        var bonus = 0.0;
        
        switch (missionType)
        {
            case MissionType.Diplomacy:
                bonus += agent.Diplomacy * 5;
                break;
            case MissionType.Espionage:
                bonus += agent.Espionage * 5;
                break;
            case MissionType.Assassination:
            case MissionType.Sabotage:
                bonus += agent.Combat * 3 + agent.Espionage * 2;
                break;
            case MissionType.Training:
                bonus += agent.Leadership * 2 + agent.Diplomacy * 3;
                break;
            default:
                bonus += agent.Leadership * 2;
                break;
        }
        
        // Force sensitivity bonus
        if (agent.IsForceSensitive) bonus += 10;
        if (agent.ForceAlignment == ForceAlignment.Light || agent.ForceAlignment == ForceAlignment.Dark) bonus += 20;
        
        return bonus;
    }

    public double CalculateEnvironmentalModifier(Planet target)
    {
        var modifier = 1.0;
        
        // Adjust based on planet type
        modifier *= target.PlanetType?.Name.ToLower() switch
        {
            "terran" => 1.0,
            "desert" => 0.9,
            "ice" => 0.8,
            "gas giant" => 0.7,
            "asteroid" => 0.6,
            "ocean" => 0.9,
            _ => 1.0
        };
        
        // Adjust based on structures (more structures = harder)
        modifier *= Math.Max(0.5, 1.0 - (target.Structures.Count * 0.05));
        
        return modifier;
    }

    public double CalculateSuccessChance(int difficulty, double skillBonus, double environmentalModifier)
    {
        var baseChance = 50.0;
        var adjustedChance = baseChance + skillBonus - difficulty;
        adjustedChance *= environmentalModifier;
        
        return Math.Clamp(adjustedChance, 5.0, 95.0) / 100.0;
    }

    public List<MissionReward> CalculateRewards(Mission mission, bool success, Character agent)
    {
        var rewards = new List<MissionReward>();
        
        if (!success) return rewards;
        
        // Base rewards
        rewards.Add(new MissionReward { Type = RewardType.Experience, Description = "Mission Experience", Value = 50 });
        
        // Type-specific rewards
        switch (mission.Type)
        {
            case MissionType.Diplomacy:
                rewards.Add(new MissionReward { Type = RewardType.Reputation, Description = "Diplomatic Success", Value = 25 });
                break;
            case MissionType.Espionage:
                rewards.Add(new MissionReward { Type = RewardType.Intelligence, Description = "Gathered Intelligence", Value = 1 });
                break;
            case MissionType.Assassination:
                rewards.Add(new MissionReward { Type = RewardType.Experience, Description = "Combat Experience", Value = 75 });
                break;
            case MissionType.Training:
                rewards.Add(new MissionReward { Type = RewardType.Technology, Description = "Research Data", Value = 1 });
                break;
        }
        
        return rewards;
    }

    public List<MissionConsequence> ApplyMissionConsequences(Mission mission, bool success, Planet target)
    {
        var consequences = new List<MissionConsequence>();
        
        if (success) return consequences;
        
        // Failure consequences
        consequences.Add(new MissionConsequence { Type = ConsequenceType.ReputationLoss, Description = "Mission Failure", Value = 3 });
        
        // Type-specific consequences
        switch (mission.Type)
        {
            case MissionType.Assassination:
                consequences.Add(new MissionConsequence { Type = ConsequenceType.MoraleLoss, Description = "Assassination Attempt Failed", Value = 5 });
                break;
            case MissionType.Espionage:
                consequences.Add(new MissionConsequence { Type = ConsequenceType.StrategicDisadvantage, Description = "Counter-Intelligence Detected", Value = 4 });
                break;
            case MissionType.Sabotage:
                consequences.Add(new MissionConsequence { Type = ConsequenceType.ReputationLoss, Description = "Sabotage Discovered", Value = 6 });
                break;
        }
        
        return consequences;
    }

    private bool CanCharacterCompleteMission(Mission mission, Character agent)
    {
        // Check if agent is available
        if (!agent.IsAvailable || !agent.IsAlive) return false;
        
        // Check if agent meets requirements
        foreach (var requirement in mission.Requirements)
        {
            switch (requirement.Type)
            {
                case RequirementType.CharacterLevel:
                    if (agent.Level < requirement.Value) return false;
                    break;
                case RequirementType.CharacterSkill:
                    // Check if agent has required skill level
                    if (agent.GetSkillForMission(mission.Type) < requirement.Value) return false;
                    break;
                case RequirementType.CharacterType:
                    if ((int)agent.Type < requirement.Value) return false;
                    break;
                case RequirementType.ReputationLevel:
                    if (agent.Loyalty < requirement.Value) return false;
                    break;
            }
        }
        
        return true;
    }

    private CombatResult ApplyMissionResults(Mission mission, Character agent, Planet target, MissionOutcome outcome)
    {
        var result = new CombatResult
        {
            CombatId = Guid.NewGuid(),
            Type = CombatType.Mission,
            AttackerVictory = outcome == MissionOutcome.Success || outcome == MissionOutcome.CriticalSuccess,
            RoundsFought = 1,
            Duration = TimeSpan.FromMinutes(5),
            AttackerLosses = new List<CombatShip>(),
            DefenderLosses = new List<CombatShip>()
        };

        // Apply rewards and consequences based on outcome
        switch (outcome)
        {
            case MissionOutcome.CriticalSuccess:
                ApplyRewards(mission, agent, 2.0); // Double rewards
                agent.GainExperience(mission.PotentialRewards.Count * 50);
                break;

            case MissionOutcome.Success:
                ApplyRewards(mission, agent, 1.0);
                agent.GainExperience(mission.PotentialRewards.Count * 25);
                break;

            case MissionOutcome.Failure:
                ApplyConsequences(mission, agent, 1.0);
                agent.GainExperience(10); // Small experience for trying
                break;

            case MissionOutcome.CriticalFailure:
                ApplyConsequences(mission, agent, 2.0); // Double consequences
                agent.GainExperience(5); // Minimal experience
                break;
        }

        // Update mission status
        mission.Status = result.AttackerVictory ? MissionStatus.Completed : MissionStatus.Failed;
        mission.CompletionTime = DateTime.UtcNow;

        // Update character status
        agent.IsAvailable = true;
        agent.CurrentMissionId = null;

        return result;
    }

    private void ApplyRewards(Mission mission, Character agent, double multiplier)
    {
        foreach (var reward in mission.PotentialRewards)
        {
            var amount = (int)(reward.Value * multiplier);
            
            switch (reward.Type)
            {
                case RewardType.Experience:
                    agent.GainExperience(amount);
                    break;
                    
                case RewardType.Technology:
                    if (!agent.Abilities.Any(a => a.Name == reward.Description))
                    {
                        agent.Abilities.Add(new CharacterAbility { Name = reward.Description, Description = "Technology-based ability" });
                    }
                    break;
                    
                case RewardType.Intelligence:
                    // Intelligence rewards would be handled by the game state manager
                    break;
                    
                // Other reward types would be handled by the game state manager
                default:
                    _logger.LogDebug("Reward type {RewardType} applied to character {CharacterId}", 
                        reward.Type, agent.Id);
                    break;
            }
        }
    }

    private void ApplyConsequences(Mission mission, Character agent, double multiplier)
    {
        foreach (var consequence in mission.PotentialConsequences)
        {
            var severity = (int)(consequence.Value * multiplier);
            
            switch (consequence.Type)
            {
                case ConsequenceType.MoraleLoss:
                    // Reduce character stats temporarily
                    agent.Combat = Math.Max(1, agent.Combat - severity);
                    agent.Espionage = Math.Max(1, agent.Espionage - severity);
                    break;
                    
                case ConsequenceType.ReputationLoss:
                    // Reduce loyalty
                    agent.Loyalty = Math.Max(0, agent.Loyalty - severity * 5);
                    break;
                    
                case ConsequenceType.StrategicDisadvantage:
                    // Mark character as unavailable for a time
                    agent.IsAvailable = false;
                    break;
                    
                // Other consequence types would be handled by the game state manager
                default:
                    _logger.LogDebug("Consequence type {ConsequenceType} applied to character {CharacterId}", 
                        consequence.Type, agent.Id);
                    break;
            }
        }
    }
}
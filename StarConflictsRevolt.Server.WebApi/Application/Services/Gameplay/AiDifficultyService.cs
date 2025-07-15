using StarConflictsRevolt.Server.WebApi.Core.Domain.Enums;
using StarConflictsRevolt.Server.WebApi.Application.Services.AI;

namespace StarConflictsRevolt.Server.WebApi.Application.Services.Gameplay;

public interface IAiDifficultyService
{
    TimeSpan GetResponseDelay(AiDifficulty difficulty);
    double GetStrategyEffectiveness(AiDifficulty difficulty);
    void AdjustAiStrategy(IAiStrategy strategy, AiDifficulty difficulty);
    bool ShouldMakeMistake(AiDifficulty difficulty);
    double GetMistakeProbability(AiDifficulty difficulty);
}

public class AiDifficultyService : IAiDifficultyService
{
    private readonly ILogger<AiDifficultyService> _logger;

    public AiDifficultyService(ILogger<AiDifficultyService> logger)
    {
        _logger = logger;
    }

    public TimeSpan GetResponseDelay(AiDifficulty difficulty)
    {
        return difficulty switch
        {
            AiDifficulty.Easy => TimeSpan.FromSeconds(Random.Shared.Next(5, 11)), // 5-10 seconds
            AiDifficulty.Normal => TimeSpan.FromSeconds(Random.Shared.Next(3, 6)), // 3-5 seconds
            AiDifficulty.Hard => TimeSpan.FromSeconds(Random.Shared.Next(1, 4)), // 1-3 seconds
            AiDifficulty.Expert => TimeSpan.FromMilliseconds(Random.Shared.Next(100, 500)), // 0.1-0.5 seconds
            _ => TimeSpan.FromSeconds(5)
        };
    }

    public double GetStrategyEffectiveness(AiDifficulty difficulty)
    {
        return difficulty switch
        {
            AiDifficulty.Easy => 0.3, // 30% effectiveness
            AiDifficulty.Normal => 0.6, // 60% effectiveness
            AiDifficulty.Hard => 0.8, // 80% effectiveness
            AiDifficulty.Expert => 0.95, // 95% effectiveness
            _ => 0.5
        };
    }

    public void AdjustAiStrategy(IAiStrategy strategy, AiDifficulty difficulty)
    {
        var effectiveness = GetStrategyEffectiveness(difficulty);
        
        // Adjust strategy parameters based on difficulty
        if (strategy is DefaultAiStrategy defaultStrategy)
        {
            // Scale decision weights by effectiveness
            defaultStrategy.AdjustDecisionWeights(effectiveness);
        }
        else if (strategy is AggressiveAiStrategy aggressiveStrategy)
        {
            aggressiveStrategy.SetAggressionLevel(difficulty == AiDifficulty.Expert ? 1.0 : effectiveness);
        }
        else if (strategy is EconomicAiStrategy economicStrategy)
        {
            economicStrategy.SetEfficiencyLevel(effectiveness);
        }
        else if (strategy is DefensiveAiStrategy defensiveStrategy)
        {
            defensiveStrategy.SetDefensiveLevel(effectiveness);
        }
        else if (strategy is BalancedAiStrategy balancedStrategy)
        {
            balancedStrategy.SetBalanceLevel(effectiveness);
        }
    }

    public bool ShouldMakeMistake(AiDifficulty difficulty)
    {
        var mistakeChance = GetMistakeProbability(difficulty);
        return Random.Shared.NextDouble() < mistakeChance;
    }

    public double GetMistakeProbability(AiDifficulty difficulty)
    {
        return difficulty switch
        {
            AiDifficulty.Easy => 0.4, // 40% chance to make mistakes
            AiDifficulty.Normal => 0.2, // 20% chance to make mistakes
            AiDifficulty.Hard => 0.05, // 5% chance to make mistakes
            AiDifficulty.Expert => 0.01, // 1% chance to make mistakes
            _ => 0.2
        };
    }
} 
using StarConflictsRevolt.Server.Domain.Enums;

namespace StarConflictsRevolt.Server.AI;

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
            AiDifficulty.Easy => TimeSpan.FromSeconds(Random.Shared.Next(5, 11)),
            AiDifficulty.Normal => TimeSpan.FromSeconds(Random.Shared.Next(3, 6)),
            AiDifficulty.Hard => TimeSpan.FromSeconds(Random.Shared.Next(1, 4)),
            AiDifficulty.Expert => TimeSpan.FromMilliseconds(Random.Shared.Next(100, 500)),
            _ => TimeSpan.FromSeconds(5)
        };
    }

    public double GetStrategyEffectiveness(AiDifficulty difficulty)
    {
        return difficulty switch
        {
            AiDifficulty.Easy => 0.3,
            AiDifficulty.Normal => 0.6,
            AiDifficulty.Hard => 0.8,
            AiDifficulty.Expert => 0.95,
            _ => 0.5
        };
    }

    public void AdjustAiStrategy(IAiStrategy strategy, AiDifficulty difficulty)
    {
        var effectiveness = GetStrategyEffectiveness(difficulty);
        if (strategy is DefaultAiStrategy defaultStrategy)
            defaultStrategy.AdjustDecisionWeights(effectiveness);
        else if (strategy is AggressiveAiStrategy aggressiveStrategy)
            aggressiveStrategy.SetAggressionLevel(difficulty == AiDifficulty.Expert ? 1.0 : effectiveness);
        else if (strategy is EconomicAiStrategy economicStrategy)
            economicStrategy.SetEfficiencyLevel(effectiveness);
        else if (strategy is DefensiveAiStrategy defensiveStrategy)
            defensiveStrategy.SetDefensiveLevel(effectiveness);
        else if (strategy is BalancedAiStrategy balancedStrategy)
            balancedStrategy.SetBalanceLevel(effectiveness);
    }

    public bool ShouldMakeMistake(AiDifficulty difficulty)
    {
        return Random.Shared.NextDouble() < GetMistakeProbability(difficulty);
    }

    public double GetMistakeProbability(AiDifficulty difficulty)
    {
        return difficulty switch
        {
            AiDifficulty.Easy => 0.4,
            AiDifficulty.Normal => 0.2,
            AiDifficulty.Hard => 0.05,
            AiDifficulty.Expert => 0.01,
            _ => 0.2
        };
    }
}

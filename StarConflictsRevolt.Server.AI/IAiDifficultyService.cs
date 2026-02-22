using StarConflictsRevolt.Server.Domain.Enums;

namespace StarConflictsRevolt.Server.AI;

public interface IAiDifficultyService
{
    TimeSpan GetResponseDelay(AiDifficulty difficulty);
    double GetStrategyEffectiveness(AiDifficulty difficulty);
    void AdjustAiStrategy(IAiStrategy strategy, AiDifficulty difficulty);
    bool ShouldMakeMistake(AiDifficulty difficulty);
    double GetMistakeProbability(AiDifficulty difficulty);
}

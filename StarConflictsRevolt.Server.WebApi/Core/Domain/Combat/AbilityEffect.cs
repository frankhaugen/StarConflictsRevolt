namespace StarConflictsRevolt.Server.WebApi.Core.Domain.Combat;

public class AbilityEffect
{
    public EffectType Type { get; set; }
    public string TargetStat { get; set; } = string.Empty;
    public double Modifier { get; set; } = 1.0;
    public int Duration { get; set; } = 1;
    public string Description { get; set; } = string.Empty;
}
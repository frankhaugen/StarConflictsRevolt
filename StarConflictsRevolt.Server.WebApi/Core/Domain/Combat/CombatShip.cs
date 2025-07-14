namespace StarConflictsRevolt.Server.WebApi.Core.Domain.Combat;

public class CombatShip
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
    public ShipCombatStats Stats { get; set; } = new();
    public bool IsAttacker { get; set; }
    public int Initiative { get; set; }
    public CombatShip? CurrentTarget { get; set; }

    public void InitializeCombat()
    {
        Stats.InitializeCombat();
        Initiative = Stats.Speed + Random.Shared.Next(1, 11); // Speed + 1d10
    }

    public void UpdateInitiative()
    {
        Initiative = Stats.Speed + Random.Shared.Next(1, 11);
    }
}
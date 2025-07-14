namespace StarConflictsRevolt.Server.WebApi.Models.Combat;

public class DeathStar
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "Death Star";
    public Guid OwnerId { get; set; }

    // Defensive capabilities
    public int ShieldStrength { get; set; } = 1000;
    public int TurbolaserBatteries { get; set; } = 50;
    public int TIEInterceptors { get; set; } = 100;
    public int ShieldGeneratorHealth { get; set; } = 500;

    // Current state
    public int CurrentShieldStrength { get; set; }
    public bool ShieldGeneratorDestroyed { get; set; }
    public bool ExhaustPortVulnerable { get; set; }
    public int ActiveTurbolaserBatteries { get; set; }
    public int ActiveTIEInterceptors { get; set; }

    public void InitializeCombat()
    {
        CurrentShieldStrength = ShieldStrength;
        ShieldGeneratorDestroyed = false;
        ExhaustPortVulnerable = false;
        ActiveTurbolaserBatteries = TurbolaserBatteries;
        ActiveTIEInterceptors = TIEInterceptors;
    }

    public List<TurbolaserShot> GenerateTurbolaserFire()
    {
        var shots = new List<TurbolaserShot>();
        var activeBatteries = ShieldGeneratorDestroyed ? ActiveTurbolaserBatteries / 2 : ActiveTurbolaserBatteries;

        for (var i = 0; i < activeBatteries; i++)
            if (Random.Shared.NextDouble() < 0.3) // 30% chance to fire
                shots.Add(new TurbolaserShot
                {
                    Accuracy = 0.7,
                    Damage = 50,
                    BatteryId = i
                });

        return shots;
    }

    public void ApplyDamage(int damage)
    {
        if (!ShieldGeneratorDestroyed)
        {
            ShieldGeneratorHealth -= damage;
            if (ShieldGeneratorHealth <= 0)
            {
                ShieldGeneratorDestroyed = true;
                ActiveTurbolaserBatteries /= 2;
            }
        }
        else
        {
            CurrentShieldStrength -= damage;
            if (CurrentShieldStrength <= 0) ExhaustPortVulnerable = true;
        }
    }
}
namespace StarConflictsRevolt.Server.WebApi.Models.Combat;

public class GroundUnitAbility
{
    public string Name { get; set; } = string.Empty;
    public AbilityType Type { get; set; }
    public int Cooldown { get; set; }
    public int CurrentCooldown { get; set; }
    public double EffectValue { get; set; }
    public bool IsActive { get; set; }

    public bool CanActivate()
    {
        return CurrentCooldown <= 0 && !IsActive;
    }

    public void Activate()
    {
        if (CanActivate())
        {
            IsActive = true;
            CurrentCooldown = Cooldown;
        }
    }

    public void UpdateCooldown()
    {
        if (CurrentCooldown > 0)
            CurrentCooldown--;
    }
}
namespace StarConflictsRevolt.Server.WebApi.Models.Combat;

public class SpecialAbility
{
    public string Name { get; set; } = string.Empty;
    public AbilityType Type { get; set; }
    public int Cooldown { get; set; }
    public int CurrentCooldown { get; set; } = 0;
    public double EffectValue { get; set; }
    public bool IsActive { get; set; } = false;
    
    public bool CanActivate() => CurrentCooldown <= 0 && !IsActive;
    
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
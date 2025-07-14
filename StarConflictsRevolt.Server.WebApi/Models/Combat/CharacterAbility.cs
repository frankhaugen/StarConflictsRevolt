namespace StarConflictsRevolt.Server.WebApi.Models.Combat;

public class CharacterAbility
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public AbilityEffect Effect { get; set; }
    public int Cooldown { get; set; } = 0;
    public int CurrentCooldown { get; set; } = 0;
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
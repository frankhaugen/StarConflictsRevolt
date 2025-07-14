namespace StarConflictsRevolt.Server.WebApi.Core.Domain.Combat;

public class CharacterAbility
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public AbilityEffect Effect { get; set; }
    public int Cooldown { get; set; } = 0;
    public int CurrentCooldown { get; set; }
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
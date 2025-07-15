namespace StarConflictsRevolt.Server.WebApi.Core.Domain.Combat;

public class TIEFighter
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public double Accuracy { get; set; } = 0.6;
    public int Damage { get; set; } = 25;
    public DateTime DeployedAt { get; set; } = DateTime.UtcNow;
    public bool Destroyed { get; set; }
    public Guid? TargetShipId { get; set; }
    public TIEFighterType Type { get; set; } = TIEFighterType.Interceptor;
}

public enum TIEFighterType
{
    Fighter,
    Interceptor,
    Bomber,
    Advanced
}
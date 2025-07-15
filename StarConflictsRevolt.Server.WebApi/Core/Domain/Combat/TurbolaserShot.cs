namespace StarConflictsRevolt.Server.WebApi.Core.Domain.Combat;

public class TurbolaserShot
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int BatteryId { get; set; }
    public double Accuracy { get; set; } = 0.7;
    public int Damage { get; set; } = 50;
    public DateTime FiredAt { get; set; } = DateTime.UtcNow;
    public bool Hit { get; set; }
    public Guid? TargetShipId { get; set; }
}
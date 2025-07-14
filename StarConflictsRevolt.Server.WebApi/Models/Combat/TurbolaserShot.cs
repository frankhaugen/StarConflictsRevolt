namespace StarConflictsRevolt.Server.WebApi.Models.Combat;

public class TurbolaserShot
{
    public int BatteryId { get; set; }
    public double Accuracy { get; set; }
    public int Damage { get; set; }
    public bool Hit { get; set; }
    public Guid? TargetShipId { get; set; }
}
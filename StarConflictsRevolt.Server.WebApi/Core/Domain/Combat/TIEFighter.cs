namespace StarConflictsRevolt.Server.WebApi.Models.Combat;

public class TIEFighter
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public double Accuracy { get; set; } = 0.6;
    public int Damage { get; set; } = 15;
    public bool IsDestroyed { get; set; } = false;
    public Guid? TargetShipId { get; set; }
}
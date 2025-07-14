namespace StarConflictsRevolt.Server.WebApi.Models.Combat;

public class GroundCombat
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AttackerId { get; set; }
    public Guid DefenderId { get; set; }
    public Planet Location { get; set; } = new("Combat Location", 0, 0, 0, 0, 0, new List<Fleet>(), new List<Structure>(), null, 0, 0, 0, 0, 0, 0, 0, PlanetType.Terran);

    // Ground forces
    public List<GroundUnit> AttackerUnits { get; set; } = new();
    public List<GroundUnit> DefenderUnits { get; set; } = new();

    // Combat state
    public GroundCombatState State { get; set; } = new();
    public List<GroundCombatRound> Rounds { get; set; } = new();
    public GroundCombatResult? Result { get; set; }

    public bool IsCombatEnded => Result != null || State.CurrentRound >= State.MaxRounds;

    public List<GroundUnit> GetAllUnits()
    {
        return AttackerUnits.Concat(DefenderUnits).ToList();
    }

    public List<GroundUnit> GetActiveUnits()
    {
        return GetAllUnits().Where(u => !u.IsDestroyed).ToList();
    }
}
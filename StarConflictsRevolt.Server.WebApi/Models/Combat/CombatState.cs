namespace StarConflictsRevolt.Server.WebApi.Models.Combat;

public class CombatState
{
    public Guid CombatId { get; set; } = Guid.NewGuid();
    public CombatType Type { get; set; }
    public DateTime StartTime { get; set; } = DateTime.UtcNow;
    public int CurrentRound { get; set; } = 0;
    public int MaxRounds { get; set; } = 20;
    
    // Combatants
    public List<CombatShip> AttackerShips { get; set; } = new();
    public List<CombatShip> DefenderShips { get; set; } = new();
    
    // Combat environment
    public Planet? Location { get; set; }
    public CombatEnvironment Environment { get; set; } = new();
    
    // Combat results
    public List<CombatRound> Rounds { get; set; } = new();
    public CombatResult? FinalResult { get; set; }
    
    public bool IsCombatEnded => FinalResult != null || CurrentRound >= MaxRounds;
    
    public List<CombatShip> GetAllShips()
    {
        return AttackerShips.Concat(DefenderShips).ToList();
    }
    
    public List<CombatShip> GetActiveShips()
    {
        return GetAllShips().Where(s => !s.Stats.IsDestroyed).ToList();
    }
    
    public void AddRound(CombatRound round)
    {
        Rounds.Add(round);
        CurrentRound++;
    }
}
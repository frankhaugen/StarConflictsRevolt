namespace StarConflictsRevolt.Server.WebApi.Models.Combat;

public class BombardmentResult
{
    public bool Success { get; set; }
    public int StructureDamage { get; set; }
    public int PopulationCasualties { get; set; }
    public int DefenseCasualties { get; set; }
    public List<Structure> DestroyedStructures { get; set; } = new();
    public bool ShieldGeneratorDestroyed { get; set; } = false;
    public string Description { get; set; } = string.Empty;
}
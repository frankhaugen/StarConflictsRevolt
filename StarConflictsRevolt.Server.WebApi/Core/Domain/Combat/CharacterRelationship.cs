namespace StarConflictsRevolt.Server.WebApi.Models.Combat;

public class CharacterRelationship
{
    public Guid CharacterId { get; set; }
    public RelationshipType Type { get; set; }
    public int Strength { get; set; } = 0; // -100 to +100
    public string Description { get; set; } = string.Empty;
}
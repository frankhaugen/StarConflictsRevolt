namespace StarConflictsRevolt.Server.Core;

public class StructureType : IGameObject
{
    /// <inheritdoc />
    public Guid Id { get; set; }
    
    /// <inheritdoc />
    public StructureVariant Variant { get; set; }
}
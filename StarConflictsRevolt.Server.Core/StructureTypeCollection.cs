using System.Collections;

namespace StarConflictsRevolt.Server.Core;

public class StructureTypeCollection : IEnumerable<StructureType>
{
    public static readonly StructureType ConstructionYard = new StructureType
    {
        Id = new Guid("00000000-0000-0000-0000-200000000001"),
        Variant = StructureVariant.ConstructionYard
    };
    public static readonly StructureType TrainingFacility = new StructureType
    {
        Id = new Guid("00000000-0000-0000-0000-200000000002"),
        Variant = StructureVariant.TrainingFacility
    };
    public static readonly StructureType Shipyard = new StructureType
    {
        Id = new Guid("00000000-0000-0000-0000-200000000003"),
        Variant = StructureVariant.Shipyard
    };
    public static readonly StructureType Mine = new StructureType
    {
        Id = new Guid("00000000-0000-0000-0000-200000000004"),
        Variant = StructureVariant.Mine
    };
    public static readonly StructureType Refinery = new StructureType
    {
        Id = new Guid("00000000-0000-0000-0000-200000000005"),
        Variant = StructureVariant.Refinery
    };
    public static readonly StructureType ShieldGenerator = new StructureType
    {
        Id = new Guid("00000000-0000-0000-0000-200000000006"),
        Variant = StructureVariant.ShieldGenerator
    };

    public IEnumerator<StructureType> GetEnumerator()
    {
        yield return ConstructionYard;
        yield return TrainingFacility;
        yield return Shipyard;
        yield return Mine;
        yield return Refinery;
        yield return ShieldGenerator;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
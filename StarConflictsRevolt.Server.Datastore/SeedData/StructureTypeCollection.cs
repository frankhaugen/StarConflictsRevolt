using System.Collections;
using StarConflictsRevolt.Server.Core.Enums;
using StarConflictsRevolt.Server.Datastore.Entities;

namespace StarConflictsRevolt.Server.Datastore.SeedData;

public class StructureCollection : IEnumerable<Structure>
{
    public static readonly Structure ConstructionYard = new Structure
    {
        Id = new Guid("00000000-0000-0000-0000-200000000001"),
        Variant = StructureVariant.ConstructionYard
    };
    public static readonly Structure TrainingFacility = new Structure
    {
        Id = new Guid("00000000-0000-0000-0000-200000000002"),
        Variant = StructureVariant.TrainingFacility
    };
    public static readonly Structure Shipyard = new Structure
    {
        Id = new Guid("00000000-0000-0000-0000-200000000003"),
        Variant = StructureVariant.Shipyard
    };
    public static readonly Structure Mine = new Structure
    {
        Id = new Guid("00000000-0000-0000-0000-200000000004"),
        Variant = StructureVariant.Mine
    };
    public static readonly Structure Refinery = new Structure
    {
        Id = new Guid("00000000-0000-0000-0000-200000000005"),
        Variant = StructureVariant.Refinery
    };
    public static readonly Structure ShieldGenerator = new Structure
    {
        Id = new Guid("00000000-0000-0000-0000-200000000006"),
        Variant = StructureVariant.ShieldGenerator
    };

    public IEnumerator<Structure> GetEnumerator()
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
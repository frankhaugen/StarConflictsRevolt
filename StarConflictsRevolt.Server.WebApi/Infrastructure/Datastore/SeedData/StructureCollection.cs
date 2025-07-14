using System.Collections;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Enums;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Gameplay;

namespace StarConflictsRevolt.Server.WebApi.Infrastructure.Datastore.SeedData;

public class StructureCollection : IEnumerable<Structure>
{
    public static readonly Structure ConstructionYard = new()
    {
        Id = new Guid("00000000-0000-0000-0000-200000000001"),
        Variant = StructureVariant.ConstructionYard
    };

    public static readonly Structure TrainingFacility = new()
    {
        Id = new Guid("00000000-0000-0000-0000-200000000002"),
        Variant = StructureVariant.TrainingFacility
    };

    public static readonly Structure Shipyard = new()
    {
        Id = new Guid("00000000-0000-0000-0000-200000000003"),
        Variant = StructureVariant.Shipyard
    };

    public static readonly Structure Mine = new()
    {
        Id = new Guid("00000000-0000-0000-0000-200000000004"),
        Variant = StructureVariant.Mine
    };

    public static readonly Structure Refinery = new()
    {
        Id = new Guid("00000000-0000-0000-0000-200000000005"),
        Variant = StructureVariant.Refinery
    };

    public static readonly Structure ShieldGenerator = new()
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

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
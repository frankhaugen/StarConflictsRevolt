using System.Collections;
using System.Collections.Generic;

namespace StarConflictsRevolt.Server.Core;

public enum StructureType
{
    ConstructionYard,
    TrainingFacility,
    Shipyard,
    Mine,
    Refinery,
    ShieldGenerator
}

public class StructureTypeCollection : IEnumerable<StructureType>
{
    public static readonly StructureType ConstructionYard = StructureType.ConstructionYard;
    public static readonly StructureType TrainingFacility = StructureType.TrainingFacility;
    public static readonly StructureType Shipyard = StructureType.Shipyard;
    public static readonly StructureType Mine = StructureType.Mine;
    public static readonly StructureType Refinery = StructureType.Refinery;
    public static readonly StructureType ShieldGenerator = StructureType.ShieldGenerator;

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
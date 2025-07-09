using System.Collections;

namespace StarConflictsRevolt.Server.Core;

public class FleetCollection : IEnumerable<Fleet>
{
    public static readonly Fleet RebelFlagship = new(
        Guid.Parse("00000000-0000-0000-0000-000000000001"),
        "Rebel Flagship",
        new List<Ship> { ShipCollection.XWing, ShipCollection.YWing },
        FleetStatus.Idle,
        null
    );

    public static readonly Fleet ImperialFleet = new(
        Guid.Parse("00000000-0000-0000-0000-000000000002"),
        "Imperial Fleet",
        new List<Ship> { ShipCollection.TIEFighter, ShipCollection.StarDestroyer },
        FleetStatus.Idle,
        null
    );

    public IEnumerator<Fleet> GetEnumerator()
    {
        yield return RebelFlagship;
        yield return ImperialFleet;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
} 
using System.Collections;
using System.Collections.Generic;

namespace StarConflictsRevolt.Server.Core;

public class ShipCollection : IEnumerable<Ship>
{
    public static readonly Ship XWing = new(
        Guid.Parse("10000000-0000-0000-0000-000000000001"),
        "X-Wing",
        new HyperdriveRating(1.0f, 0.8f),
        false
    );

    public static readonly Ship YWing = new(
        Guid.Parse("10000000-0000-0000-0000-000000000002"),
        "Y-Wing",
        new HyperdriveRating(1.2f, 1.0f),
        false
    );

    public static readonly Ship TIEFighter = new(
        Guid.Parse("10000000-0000-0000-0000-000000000003"),
        "TIE Fighter",
        new HyperdriveRating(1.5f, 1.3f),
        false
    );

    public static readonly Ship StarDestroyer = new(
        Guid.Parse("10000000-0000-0000-0000-000000000004"),
        "Star Destroyer",
        new HyperdriveRating(0.7f, 0.5f),
        false
    );

    public IEnumerator<Ship> GetEnumerator()
    {
        yield return XWing;
        yield return YWing;
        yield return TIEFighter;
        yield return StarDestroyer;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
} 
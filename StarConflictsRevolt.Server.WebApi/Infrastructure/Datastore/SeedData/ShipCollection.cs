using System.Collections;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Gameplay;

namespace StarConflictsRevolt.Server.WebApi.Infrastructure.Datastore.SeedData;

public class ShipCollection : IEnumerable<Ship>
{
    public static readonly Ship XWing = new(
        Guid.Parse("10000000-0000-0000-0000-000000000001"),
        "X-Wing",
        false
    );

    public static readonly Ship YWing = new(
        Guid.Parse("10000000-0000-0000-0000-000000000002"),
        "Y-Wing",
        false
    );

    public static readonly Ship TIEFighter = new(
        Guid.Parse("10000000-0000-0000-0000-000000000003"),
        "TIE Fighter",
        false
    );

    public static readonly Ship StarDestroyer = new(
        Guid.Parse("10000000-0000-0000-0000-000000000004"),
        "Star Destroyer",
        false
    );

    public IEnumerator<Ship> GetEnumerator()
    {
        yield return XWing;
        yield return YWing;
        yield return TIEFighter;
        yield return StarDestroyer;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
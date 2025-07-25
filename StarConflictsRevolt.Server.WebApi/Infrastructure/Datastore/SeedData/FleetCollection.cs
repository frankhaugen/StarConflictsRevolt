using System.Collections;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Enums;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Gameplay;

namespace StarConflictsRevolt.Server.WebApi.Infrastructure.Datastore.SeedData;

public class FleetCollection : IEnumerable<Fleet>
{
    public static readonly Fleet RebelFlagship = new(
        Guid.Parse("00000000-0000-0000-0000-000000000001"),
        "Rebel Flagship",
        new List<Ship>(),
        FleetStatus.Idle
    );

    public static readonly Fleet ImperialFleet = new(
        Guid.Parse("00000000-0000-0000-0000-000000000002"),
        "Imperial Fleet",
        new List<Ship>(),
        FleetStatus.Idle
    );

    public IEnumerator<Fleet> GetEnumerator()
    {
        yield return RebelFlagship;
        yield return ImperialFleet;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
using System.Numerics;
using StarConflictsRevolt.Clients.Models;
using StarConflictsRevolt.Server.WebApi;

namespace StarConflictsRevolt.Server.WebApi.API.Services;

/// <summary>Produces a 2D star map from real galactic star data (discified).</summary>
public interface IStarMapService
{
    Task<IReadOnlyList<StarMapPointDto>> GetStarMapAsync(StarMapQuery query, CancellationToken cancellationToken = default);
}

public sealed class StarMapQuery
{
    public double MaxDistanceParsecs { get; init; } = 500;
    public double MaxAbsZParsecs { get; init; } = 100;
    public int MaxStars { get; init; } = 4000;
}

public sealed class StarMapService : IStarMapService
{
    public async Task<IReadOnlyList<StarMapPointDto>> GetStarMapAsync(StarMapQuery query, CancellationToken cancellationToken = default)
    {
        var options = new DiscifyOptions
        {
            MaxDistanceFromSolParsecs = query.MaxDistanceParsecs,
            MaxAbsZParsecs = query.MaxAbsZParsecs,
            MaxStars = query.MaxStars,
            ScaleToRange = 1000
        };

        var systems = StarSystemHelper.LoadStarSystemsFromEmbeddedAsync(cancellationToken);
        var discified = await StarSystemHelper.DiscifyAsync(systems, options, cancellationToken);

        return discified
            .Select(p => new StarMapPointDto(p.Name, new Vector2((float)p.MapX, (float)p.MapY)))
            .ToList();
    }
}

using StarConflictsRevolt.Server.WebApi.API.Services;

namespace StarConflictsRevolt.Server.WebApi.API.Handlers.Endpoints;

/// <summary>2D star map from real galaxy data (discified).</summary>
public static class StarMapEndpointHandler
{
    public static void MapEndpoints(WebApplication app)
    {
        app.MapGet("/game/starmap", async (
            IStarMapService starMapService,
            double? maxDistance,
            double? maxZ,
            int? maxStars,
            CancellationToken ct) =>
        {
            var query = new StarMapQuery
            {
                MaxDistanceParsecs = maxDistance ?? 500,
                MaxAbsZParsecs = maxZ ?? 100,
                MaxStars = maxStars ?? 4000
            };
            var points = await starMapService.GetStarMapAsync(query, ct);
            return Results.Ok(points);
        }).WithName("GetStarMap").WithTags("StarMap");
    }
}

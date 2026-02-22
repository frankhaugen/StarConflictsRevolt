using StarConflictsRevolt.Server.Simulation.Engine;

namespace StarConflictsRevolt.Server.WebApi.API.Handlers.Endpoints;

/// <summary>
///     Handles health check and status endpoints
/// </summary>
public static class HealthEndpointHandler
{
    /// <summary>Max age of last tick (seconds) before ticker is considered not live.</summary>
    private const int TickerLivenessMaxAgeSeconds = 15;

    public static void MapEndpoints(WebApplication app)
    {
        app.MapGet("/", async context => { await context.Response.WriteAsync("Welcome to Star Conflicts Revolt API!"); });

        app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }));

        app.MapGet("/health/game", () => Results.Text("Game on!"));

        app.MapGet("/health/ticker", (ITickerLiveness tickerLiveness) =>
        {
            var lastTick = tickerLiveness.LastTickUtc;
            if (lastTick == null)
                return Results.Json(new { Status = "Unhealthy", Reason = "Ticker has not run yet", LastTickUtc = (DateTime?)null }, statusCode: 503);
            var age = (DateTime.UtcNow - lastTick.Value).TotalSeconds;
            if (age > TickerLivenessMaxAgeSeconds)
                return Results.Json(new { Status = "Unhealthy", Reason = "Ticker stale", LastTickUtc = lastTick, AgeSeconds = age }, statusCode: 503);
            return Results.Ok(new { Status = "Healthy", LastTickUtc = lastTick, AgeSeconds = age });
        }).WithName("TickerLiveness").AllowAnonymous();
    }
}
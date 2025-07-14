namespace StarConflictsRevolt.Server.WebApi.API.Handlers.Endpoints;

/// <summary>
///     Handles health check and status endpoints
/// </summary>
public static class HealthEndpointHandler
{
    public static void MapEndpoints(WebApplication app)
    {
        app.MapGet("/", async context => { await context.Response.WriteAsync("Welcome to Star Conflicts Revolt API!"); });

        app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }));

        app.MapGet("/health/game", () => Results.Text("Game on!"));
    }
}
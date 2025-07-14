using StarConflictsRevolt.Server.WebApi.Application.Services.Gameplay;

namespace StarConflictsRevolt.Server.WebApi.API.Handlers.Endpoints;

/// <summary>
///     Handles leaderboard endpoints
/// </summary>
public static class LeaderboardEndpointHandler
{
    public static void MapEndpoints(WebApplication app)
    {
        app.MapGet("/leaderboard/{sessionId}", async (Guid sessionId, LeaderboardService leaderboardService, CancellationToken ct) =>
        {
            var leaderboard = await leaderboardService.GetLeaderboardAsync(sessionId, ct);
            return Results.Ok(leaderboard);
        }).RequireAuthorization();

        app.MapGet("/leaderboard/{sessionId}/player/{playerId}", async (Guid sessionId, Guid playerId, LeaderboardService leaderboardService, CancellationToken ct) =>
        {
            var stats = await leaderboardService.GetPlayerStatsAsync(sessionId, playerId, ct);
            if (stats == null)
                return Results.NotFound();
            return Results.Ok(stats);
        }).RequireAuthorization();

        app.MapGet("/leaderboard/top", async (LeaderboardService leaderboardService, int count = 10, CancellationToken ct = default) =>
        {
            var topPlayers = await leaderboardService.GetTopPlayersAsync(count, ct);
            return Results.Ok(topPlayers);
        }).RequireAuthorization();
    }
}
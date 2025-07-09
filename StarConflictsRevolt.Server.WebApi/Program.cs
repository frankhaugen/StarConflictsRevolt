using StarConflictsRevolt.Server.WebApi;

var builder = WebApplication.CreateBuilder(args);

WebApiStartupHelper.RegisterServices(builder);

var app = builder.Build();

WebApiStartupHelper.Configure(app);

// Leaderboard endpoints
app.MapGet("/leaderboard/{sessionId}", async (Guid sessionId, LeaderboardService leaderboardService, CancellationToken ct) =>
{
    var leaderboard = await leaderboardService.GetLeaderboardAsync(sessionId, ct);
    return Results.Ok(leaderboard);
});

app.MapGet("/leaderboard/{sessionId}/player/{playerId}", async (Guid sessionId, Guid playerId, LeaderboardService leaderboardService, CancellationToken ct) =>
{
    var stats = await leaderboardService.GetPlayerStatsAsync(sessionId, playerId, ct);
    if (stats == null)
        return Results.NotFound();
    return Results.Ok(stats);
});

app.MapGet("/leaderboard/top", async (LeaderboardService leaderboardService, int count = 10, CancellationToken ct = default) =>
{
    var topPlayers = await leaderboardService.GetTopPlayersAsync(count, ct);
    return Results.Ok(topPlayers);
});

app.Run();
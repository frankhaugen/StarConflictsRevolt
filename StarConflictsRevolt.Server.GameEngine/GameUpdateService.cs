using System.Numerics;
using Microsoft.AspNetCore.SignalR;
using StarConflictsRevolt.Clients.Models;

namespace StarConflictsRevolt.Server.GameEngine;

public class GameUpdateService : BackgroundService
{
    private readonly IHubContext<WorldHub> _hubContext;
    private readonly ILogger<GameUpdateService> _logger;

    public GameUpdateService(IHubContext<WorldHub> hubContext, ILogger<GameUpdateService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var planets = new List<PlanetDto>
            {
                new PlanetDto(Guid.CreateVersion7(), "Earth", 50, 1.0f, 0.5f, 0.1f, 0.01f)
            };
            
            var starSystems = new List<StarSystemDto>
            {
                new StarSystemDto(Guid.CreateVersion7(), "Sol", planets, new Vector2(100, 100))
            };

            var world = new WorldDto(Guid.CreateVersion7(), new GalaxyDto(Guid.CreateVersion7(), starSystems));
            _logger.LogInformation("Sending full world update: {WorldId}, StarSystems: {StarSystemCount}",
                world.Id, world.Galaxy?.StarSystems?.Count() ?? 0);
            
            await _hubContext.Clients.All.SendAsync("FullWorld", world, stoppingToken);
            _logger.LogInformation("Full world update sent: {WorldId}", world.Id);
            
            // Wait for a while before the next update
            _logger.LogInformation("Waiting for next update cycle...");
            await Task.Delay(5000, stoppingToken);
        }
    }
}
using System.Numerics;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using StarConflictsRevolt.Clients.Models;
using StarConflictsRevolt.Clients.Shared;
using StarConflictsRevolt.GameEngine;

public class GameUpdateService : BackgroundService
{
    private readonly IHubContext<WorldHub> _hubContext;

    public GameUpdateService(IHubContext<WorldHub> hubContext)
    {
        _hubContext = hubContext;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Simulate a planet update
            // var planet = new PlanetDto {
            //     Id = Guid.NewGuid()
            // };
            //
            // var updates = new List<GameObjectUpdate>
            // {
            //     new GameObjectUpdate()
            //     {
            //         Id = planet.Id,
            //         Type = UpdateType.Added,
            //         Data = JsonSerializer.SerializeToElement(planet)
            //     }
            // };
            //
            // await _hubContext.Clients.All.SendAsync("ReceiveUpdates", updates, stoppingToken);
            // await Task.Delay(1000, stoppingToken);
            
            // FullWorld update example
            
            var planets = new List<PlanetDto>
            {
                new PlanetDto(Guid.CreateVersion7(), "Earth", 50, 1.0f, 0.5f, 0.1f, 0.01f)
            };
            
            var starSystems = new List<StarSystemDto>
            {
                new StarSystemDto(Guid.CreateVersion7(), "Sol", planets, new Vector2(100, 100))
            };

            var world = new WorldDto(Guid.CreateVersion7(), new GalaxyDto(Guid.CreateVersion7(), starSystems));
            await _hubContext.Clients.All.SendAsync("FullWorld", world, stoppingToken);
            // Wait for a while before the next update
            await Task.Delay(5000, stoppingToken);
        }
    }
}
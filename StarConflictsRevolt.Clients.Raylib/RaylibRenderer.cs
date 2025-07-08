using Raylib_CSharp;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Windowing;
using StarConflictsRevolt.Clients.Models;
using StarConflictsRevolt.Clients.Shared;

namespace StarConflictsRevolt.Clients.Raylib;

public class RaylibRenderer : IGameRenderer, IAsyncDisposable
{
    private readonly ILogger<RaylibRenderer> _logger;
    
    public RaylibRenderer(ILogger<RaylibRenderer> logger)
    {
        _logger = logger;
        Window.Init(800, 600, "Star Conflicts Revolt");
        Time.SetTargetFPS(60);
    }
    
    public Task RenderAsync(WorldDto world, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("Render cancelled");
            return Task.CompletedTask;
        }
        
        if (world == null)
        {
            _logger.LogWarning("Render called with null world");
            return Task.CompletedTask;
        }
        
        _logger.LogInformation("Rendering world: {WorldId}, StarSystems: {StarSystemCount}",
            world.Id, world.Galaxy?.StarSystems?.Count() ?? 0);
        if (world.Galaxy?.StarSystems == null || !world.Galaxy.StarSystems.Any())
        {
            _logger.LogWarning("No star systems to render in world: {WorldId}", world.Id);
            return Task.CompletedTask;
        }
        
        Graphics.BeginDrawing();
        Graphics.ClearBackground(Color.Black);

        DrawWorld(world);

        Graphics.EndDrawing();
        _logger.LogInformation("Finished rendering world: {WorldId}", world.Id);
        
        return Task.CompletedTask;
    }
    
    private void DrawWorld(WorldDto world)
    {
        // Example: Draw planets
        if (world.Galaxy?.StarSystems != null)
        {
            foreach (var system in world.Galaxy.StarSystems)
            {
                Graphics.DrawCircle((int)system.Coordinates.X, (int)system.Coordinates.Y, 20, Color.Blue);
                Graphics.DrawText(system.Name, (int)system.Coordinates.X - 20, (int)system.Coordinates.Y - 30, 10, Color.LightGray);
            }
        }
        // TODO: Draw stars, ships, etc.
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        Window.Close();
    }
}
using Raylib_CSharp;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Windowing;
using StarConflictsRevolt.Clients.Shared;

namespace StarConflictsRevolt.Clients.Raylib;

public class RaylibRenderer : IGameRenderer, IAsyncDisposable
{
    public RaylibRenderer()
    {
        Window.Init(800, 600, "Star Conflicts Revolt");
        Time.SetTargetFPS(60);
    }
    
    public Task RenderAsync(WorldDto world, CancellationToken cancellationToken)
    {
        Graphics.BeginDrawing();
        Graphics.ClearBackground(Color.Black);

        DrawWorld(world);

        Graphics.EndDrawing();
        
        return Task.CompletedTask;
    }
    
    private void DrawWorld(WorldDto world)
    {
        // Example: Draw planets
        if (world.Galaxy.StarSystems != null)
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
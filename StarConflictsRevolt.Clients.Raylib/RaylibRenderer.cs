using Raylib_CSharp;
using Raylib_CSharp.Camera.Cam2D;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Interact;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Windowing;
using StarConflictsRevolt.Clients.Models;
using StarConflictsRevolt.Clients.Shared;
using System.Numerics;

namespace StarConflictsRevolt.Clients.Raylib;

public class RaylibRenderer : IGameRenderer, IAsyncDisposable
{
    private readonly ILogger<RaylibRenderer> _logger;
    private readonly List<Vector2> _backgroundStars = new();
    private Camera2D _camera;

    public RaylibRenderer(ILogger<RaylibRenderer> logger)
    {
        _logger = logger;
        Window.Init(800, 600, "Star Conflicts Revolt");
        Window.SetState(ConfigFlags.ResizableWindow | ConfigFlags.TopmostWindow);
        Graphics.ClearBackground(Color.Black);
        Input.EnableCursor();
        Input.SetExitKey(KeyboardKey.Escape);
        Input.SetMouseCursor(MouseCursor.Crosshair);
        Time.SetTargetFPS(60);
        
        _camera = new Camera2D(new Vector2(Window.GetScreenWidth() / 2f, Window.GetScreenHeight() / 2f), new Vector2(0, 0), 0.0f, 1.0f);
        
        var random = new Random();
        for (var i = 0; i < 500; i++)
        {
            _backgroundStars.Add(new Vector2(random.Next(-2000, 2000), random.Next(-2000, 2000)));
        }
    }
    
    public Task<bool> RenderAsync(WorldDto world, CancellationToken cancellationToken)
    {
        if (Window.ShouldClose() || cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("Render cancelled");
            return Task.FromResult(false);
        }
        
        if (world.Galaxy?.StarSystems == null || !world.Galaxy.StarSystems.Any())
        {
            _logger.LogWarning("No star systems to render in world: {WorldId}", world.Id);
            return Task.FromResult(true);
        }

        HandleCameraInput();
        
        Graphics.BeginDrawing();
        Graphics.ClearBackground(Color.Black);

        Graphics.BeginMode2D(_camera);
        
        DrawBackground();
        DrawWorld(world);

        Graphics.EndMode2D();

        Graphics.EndDrawing();
        
        return Task.FromResult(true);
    }

    private void HandleCameraInput()
    {
        if (Input.IsMouseButtonDown(MouseButton.Left))
        {
            var delta = Input.GetMouseDelta();
            if (delta.LengthSquared() > 0)
            {
                var currentPos = Window.GetPosition();
                Window.SetPosition((int)(currentPos.X + delta.X), (int)(currentPos.Y + delta.Y));
            }
        }

        if (Input.IsMouseButtonDown(MouseButton.Middle))
        {
            var delta = Input.GetMouseDelta();
            delta = RayMath.Vector2Scale(delta, -1.0f / _camera.Zoom);
            _camera.Target = RayMath.Vector2Add(_camera.Target, delta);
        }

        var wheel = Input.GetMouseWheelMove();
        if (wheel != 0)
        {
            var mouseWorldPos = GetScreenToWorld2D(Input.GetMousePosition(), _camera);
            _camera.Offset = Input.GetMousePosition();
            _camera.Target = mouseWorldPos;
            _camera.Zoom += wheel * 0.125f;
            if (_camera.Zoom < 0.1f)
            {
                _camera.Zoom = 0.1f;
            }
        }
    }

    private Vector2 GetScreenToWorld2D(Vector2 getMousePosition, Camera2D camera)
    {
        var screenToWorld = RayMath.Vector2Subtract(getMousePosition, camera.Offset);
        screenToWorld = RayMath.Vector2Scale(screenToWorld, 1.0f / camera.Zoom);
        screenToWorld = RayMath.Vector2Add(screenToWorld, camera.Target);
        return screenToWorld;
    }

    private void DrawBackground()
    {
        foreach (var star in _backgroundStars)
        {
            Graphics.DrawPixelV(star, Color.White);
        }
    }
    
    private void DrawWorld(WorldDto world)
    {
        if (world.Galaxy?.StarSystems != null)
        {
            foreach (var system in world.Galaxy.StarSystems)
            {
                DrawStar(system);
                foreach (var planet in system.Planets)
                {
                    DrawPlanet(planet, system);
                }
            }
        }
    }

    private void DrawStar(StarSystemDto system)
    {
        Graphics.DrawCircle((int)system.Coordinates.X, (int)system.Coordinates.Y, (float)10, Color.Yellow);
        Graphics.DrawText(system.Name, (int)system.Coordinates.X - 20, (int)system.Coordinates.Y - 20, 10, Color.White);
    }

    private void DrawPlanet(PlanetDto planet, StarSystemDto system)
    {
        var planetX = (int)(system.Coordinates.X + planet.DistanceFromSun * Math.Cos(Time.GetTime() * planet.OrbitSpeed));
        var planetY = (int)(system.Coordinates.Y + planet.DistanceFromSun * Math.Sin(Time.GetTime() * planet.OrbitSpeed));
        Graphics.DrawCircle(planetX, planetY, (float)planet.Radius, Color.Blue);
        Graphics.DrawText(planet.Name, planetX - 10, planetY - 10, 8, Color.LightGray);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        Window.Close();
    }
}
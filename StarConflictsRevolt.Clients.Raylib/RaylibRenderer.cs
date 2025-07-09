using Raylib_CSharp;
using Raylib_CSharp.Camera.Cam2D;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Interact;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Windowing;
using StarConflictsRevolt.Clients.Models;
using System.Numerics;
using StarConflictsRevolt.Clients.Raylib.Renderers;
using StarConflictsRevolt.Clients.Shared;

namespace StarConflictsRevolt.Clients.Raylib;

public class RaylibRenderer : IGameRenderer, IAsyncDisposable
{
    private readonly ILogger<RaylibRenderer> _logger;
    private readonly List<Vector2> _backgroundStars = new();
    private Camera2D _camera;
    private bool _running = false;
    private readonly IEnumerable<IView> _views;
    private readonly RenderContext _renderContext;

    public RaylibRenderer(ILogger<RaylibRenderer> logger, IEnumerable<IView> views, RenderContext renderContext)
    {
        _logger = logger;
        _views = views;
        _renderContext = renderContext;
        _camera = new Camera2D(new Vector2(Window.GetScreenWidth() / 2f, Window.GetScreenHeight() / 2f), new Vector2(0, 0), 0.0f, 1.0f);
        
        var random = new Random();
        for (var i = 0; i < 500; i++)
        {
            _backgroundStars.Add(new Vector2(random.Next(-2000, 2000), random.Next(-2000, 2000)));
        }
        
        _logger.LogInformation("RaylibRenderer initialized with {StarCount} background stars", _backgroundStars.Count);
        Task.Run(RenderLoop);
    }
    
    public Task<bool> RenderAsync(WorldDto? world, CancellationToken cancellationToken)
    {
        return Task.FromResult(true);
    }
    
    private void RenderLoop()
    {
        _logger.LogInformation("Starting Raylib render loop");
        
        var aspectRatio = Window.GetScreenWidth() / Window.GetScreenHeight();
        var height = 1280;
        var width = (height * aspectRatio);
        
        Window.Init(width, height, "Star Conflicts Revolt");
        Input.SetExitKey(KeyboardKey.Backspace);
        Time.SetTargetFPS(60);
        
        while (!Window.ShouldClose())
        {
            // Update
            HandleCameraInput();

            // Draw
            Graphics.BeginDrawing();
            Graphics.ClearBackground(Color.Black);
            
            Graphics.BeginMode2D(_camera);
            DrawBackground();
            var viewType = _renderContext.CurrentView;
            if (viewType != null && _views.Any(v => v.ViewType == viewType))
            {
                var view = _views.First(v => v.ViewType == viewType);
                view.Draw();
            }
            Graphics.EndMode2D();
            
            Graphics.DrawText($"Zoom: {_camera.Zoom:F2}", 10, 10, 20, Color.White);

            Graphics.EndDrawing();
        }
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
            var mouseWorldPos = RayLibHelper.GetScreenToWorld2D(Input.GetMousePosition(), _camera);
            _camera.Offset = Input.GetMousePosition();
            _camera.Target = mouseWorldPos;
            _camera.Zoom += wheel * 0.125f;
            if (_camera.Zoom < 0.1f)
            {
                _camera.Zoom = 0.1f;
            }
        }
    }

    private void DrawBackground()
    {
        foreach (var star in _backgroundStars)
        {
            Graphics.DrawPixelV(star, Color.White);
        }
    }
    
    private void DrawStar(StarSystemDto system)
    {
        Graphics.DrawCircle((int)system.Coordinates.X, (int)system.Coordinates.Y, (float)10, Color.Yellow);
        Graphics.DrawText(system.Name, (int)system.Coordinates.X - 20, (int)system.Coordinates.Y - 20, 10, Color.White);
    }


    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        Window.Close();
    }
}
using System.Numerics;
using Raylib_CSharp;
using Raylib_CSharp.Camera.Cam2D;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Interact;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Windowing;
using StarConflictsRevolt.Clients.Models;
using StarConflictsRevolt.Clients.Raylib.Http;

namespace StarConflictsRevolt.Clients.Raylib.Renderers;

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
        _logger.LogInformation("RaylibRenderer initialized with {ViewCount} views", _views.Count());
    }

    public Task<bool> RenderAsync(WorldDto? world, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting RaylibRenderer render loop");
        _running = true;
        RenderLoop();
        return Task.FromResult(true);
    }

    private void RenderLoop()
    {
        _logger.LogInformation("Initializing Raylib window");
        Window.Init(1200, 800, "Star Conflicts Revolt");
        Time.SetTargetFPS(60);
        
        _logger.LogInformation("Setting up camera");
        _camera = new Camera2D(Vector2.Zero, Vector2.Zero, 0.0f, 1.0f);
        
        _logger.LogInformation("Generating background stars");
        var random = new Random();
        for (int i = 0; i < 100; i++)
        {
            _backgroundStars.Add(new Vector2(random.Next(0, 1200), random.Next(0, 800)));
        }
        
        _logger.LogInformation("Starting render loop");
        
        while (!Window.ShouldClose() && _running)
        {
            HandleCameraInput();
            
            Graphics.BeginDrawing();
            Graphics.ClearBackground(Color.Black);
            
            DrawBackground();
            
            Graphics.BeginMode2D(_camera);
            
            // Draw current view
            var currentView = _views.FirstOrDefault(v => v.ViewType == _renderContext.CurrentView);
            if (currentView != null)
            {
                currentView.Draw();
            }
            else
            {
                _logger.LogWarning("No view found for current view type: {ViewType}", _renderContext.CurrentView);
                Graphics.DrawText($"View not found: {_renderContext.CurrentView}", 10, 10, 20, Color.White);
            }
            
            Graphics.EndMode2D();
            
            // Draw UI elements that should be in screen space
            DrawUI();
            
            Graphics.EndDrawing();
        }
        
        _logger.LogInformation("Render loop ended, closing window");
        Window.Close();
    }

    private void HandleCameraInput()
    {
        var mouseDelta = Input.GetMouseDelta();
        if (Input.IsMouseButtonDown(MouseButton.Right))
        {
            _camera.Target += mouseDelta;
            _logger.LogDebug("Camera moved by delta: {Delta}", mouseDelta);
        }
        
        var scroll = Input.GetMouseWheelMove();
        if (scroll != 0)
        {
            _camera.Zoom += scroll * 0.1f;
            _camera.Zoom = Math.Max(0.1f, Math.Min(3.0f, _camera.Zoom));
            _logger.LogDebug("Camera zoom changed to: {Zoom}", _camera.Zoom);
        }
    }

    private void DrawBackground()
    {
        foreach (var star in _backgroundStars)
        {
            Graphics.DrawPixelV(star, Color.White);
        }
    }

    private void DrawUI()
    {
        // Draw UI elements in screen space
        Graphics.DrawText($"FPS: {Time.GetFPS()}", 10, 10, 20, Color.White);
        Graphics.DrawText($"View: {_renderContext.CurrentView}", 10, 35, 20, Color.White);
        
        if (_renderContext.GameState.FeedbackMessage != null)
        {
            Graphics.DrawText(_renderContext.GameState.FeedbackMessage, 10, 60, 20, Color.Yellow);
        }
    }

    private void DrawStar(StarSystemDto system)
    {
        // Simple coordinate conversion for now
        var screenPos = system.Coordinates;
        Graphics.DrawCircle((int)screenPos.X, (int)screenPos.Y, 5, Color.Yellow);
        Graphics.DrawText(system.Name, (int)screenPos.X + 10, (int)screenPos.Y - 10, 12, Color.White);
    }

    public async ValueTask DisposeAsync()
    {
        _logger.LogInformation("Disposing RaylibRenderer");
        _running = false;
        if (Window.IsReady())
        {
            Window.Close();
            _logger.LogInformation("Raylib window closed");
        }
    }
}
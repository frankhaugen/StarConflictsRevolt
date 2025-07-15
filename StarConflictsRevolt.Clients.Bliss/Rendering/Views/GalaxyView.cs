using System.Numerics;
using StarConflictsRevolt.Clients.Bliss.Core;
using StarConflictsRevolt.Clients.Bliss.Input;

namespace StarConflictsRevolt.Clients.Bliss.Rendering.Views;

/// <summary>
/// Galaxy view for the Bliss client demonstrating rendering capabilities.
/// </summary>
public class GalaxyView : IView
{
    public GameView ViewType => GameView.Galaxy;

    private readonly GameState _gameState;
    private readonly IInput _input;
    private readonly IRenderer2D _renderer;
    private readonly Random _random = new Random();
    private readonly List<Vector2> _stars = new();
    private float _time = 0;

    public GalaxyView(GameState gameState, IInput input, IRenderer2D renderer)
    {
        _gameState = gameState;
        _input = input;
        _renderer = renderer;
        GenerateStars();
    }

    public void Update(float deltaTime)
    {
        _time += deltaTime;

        // Handle camera movement
        var cameraSpeed = 200.0f * deltaTime;
        var cameraMatrix = _gameState.CameraMatrix;

        if (_input.IsKeyPressed(Key.W))
        {
            cameraMatrix.Translation += new Vector2(0, -cameraSpeed);
            _gameState.InvalidateFrame();
        }
        if (_input.IsKeyPressed(Key.S))
        {
            cameraMatrix.Translation += new Vector2(0, cameraSpeed);
            _gameState.InvalidateFrame();
        }
        if (_input.IsKeyPressed(Key.A))
        {
            cameraMatrix.Translation += new Vector2(-cameraSpeed, 0);
            _gameState.InvalidateFrame();
        }
        if (_input.IsKeyPressed(Key.D))
        {
            cameraMatrix.Translation += new Vector2(cameraSpeed, 0);
            _gameState.InvalidateFrame();
        }

        _gameState.CameraMatrix = cameraMatrix;

        // Handle mouse selection
        if (_input.IsMouseButtonJustPressed(MouseButton.Left))
        {
            var mousePos = _input.MousePosition;
            _gameState.SetFeedback($"Selected position: ({mousePos.X:F0}, {mousePos.Y:F0})");
        }
    }

    public void Draw()
    {
        // Draw animated starfield background
        DrawStarfield();

        // Draw galaxy title
        var title = "Galaxy View";
        var titlePosition = new Vector2(50, 50);
        _renderer.DrawText(title, titlePosition, Color.White, 1.5f);

        // Draw instructions
        var instructions = "WASD: Move camera | Mouse: Select | ESC: Back to menu";
        var instructionsPosition = new Vector2(50, 100);
        _renderer.DrawText(instructions, instructionsPosition, Color.Gray);

        // Draw some sample star systems
        DrawSampleStarSystems();

        // Draw camera info
        var cameraInfo = $"Camera: ({_gameState.CameraMatrix.Translation.X:F0}, {_gameState.CameraMatrix.Translation.Y:F0})";
        var cameraPosition = new Vector2(50, 150);
        _renderer.DrawText(cameraInfo, cameraPosition, Color.Cyan);
    }

    public void OnActivate()
    {
        _gameState.InvalidateFrame();
    }

    public void OnDeactivate()
    {
        // No cleanup needed
    }

    private void GenerateStars()
    {
        _stars.Clear();
        for (int i = 0; i < 1000; i++)
        {
            _stars.Add(new Vector2(
                _random.Next(-2000, 2000),
                _random.Next(-2000, 2000)
            ));
        }
    }

    private void DrawStarfield()
    {
        foreach (var star in _stars)
        {
            // Simple twinkling effect
            var twinkle = (float)(Math.Sin(_time * 2 + star.X * 0.01) * 0.5 + 0.5);
            var brightness = (byte)(100 + (int)(155 * twinkle));
            var color = new Color(brightness, brightness, brightness, 255);
            
            _renderer.DrawText("*", star, color, 0.5f);
        }
    }

    private void DrawSampleStarSystems()
    {
        var systems = new[]
        {
            new { Name = "Coruscant", Position = new Vector2(200, 200), Color = Color.Yellow },
            new { Name = "Naboo", Position = new Vector2(400, 300), Color = Color.Green },
            new { Name = "Tatooine", Position = new Vector2(600, 150), Color = Color.Orange },
            new { Name = "Hoth", Position = new Vector2(300, 500), Color = Color.Cyan },
            new { Name = "Endor", Position = new Vector2(700, 400), Color = Color.Green }
        };

        foreach (var system in systems)
        {
            // Draw star
            _renderer.DrawText("●", system.Position, system.Color, 1.0f);
            
            // Draw name
            _renderer.DrawText(system.Name, system.Position + new Vector2(15, -5), Color.White, 0.8f);
        }
    }
} 
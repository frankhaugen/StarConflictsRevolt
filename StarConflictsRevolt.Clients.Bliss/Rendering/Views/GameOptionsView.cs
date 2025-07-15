using System.Numerics;
using StarConflictsRevolt.Clients.Bliss.Core;
using StarConflictsRevolt.Clients.Bliss.Input;

namespace StarConflictsRevolt.Clients.Bliss.Rendering.Views;

/// <summary>
/// Game options view for the Bliss client.
/// </summary>
public class GameOptionsView : IView
{
    public GameView ViewType => GameView.GameOptions;

    private readonly GameState _gameState;
    private readonly IInput _input;
    private readonly IRenderer2D _renderer;
    private int _selectedOption = 0;
    private readonly string[] _options = 
    {
        "Sound: ON",
        "Fullscreen: OFF",
        "Game Speed: Normal",
        "Back to Menu"
    };

    public GameOptionsView(GameState gameState, IInput input, IRenderer2D renderer)
    {
        _gameState = gameState;
        _input = input;
        _renderer = renderer;
    }

    public void Update(float deltaTime)
    {
        // Handle navigation
        if (_input.IsKeyJustPressed(Key.Up))
        {
            _selectedOption = (_selectedOption - 1 + _options.Length) % _options.Length;
            _gameState.InvalidateFrame();
        }
        else if (_input.IsKeyJustPressed(Key.Down))
        {
            _selectedOption = (_selectedOption + 1) % _options.Length;
            _gameState.InvalidateFrame();
        }
        else if (_input.IsKeyJustPressed(Key.Enter))
        {
            HandleSelection();
        }
        else if (_input.IsKeyJustPressed(Key.Left) || _input.IsKeyJustPressed(Key.Right))
        {
            HandleOptionToggle();
        }
    }

    public void Draw()
    {
        // Draw title
        var title = "Game Options";
        var titlePosition = new Vector2(400, 100);
        _renderer.DrawText(title, titlePosition, Color.White, 2.0f);

        // Draw options
        var startY = 250;
        var spacing = 50;

        for (int i = 0; i < _options.Length; i++)
        {
            var position = new Vector2(400, startY + i * spacing);
            var color = i == _selectedOption ? Color.Yellow : Color.White;
            var prefix = i == _selectedOption ? "> " : "  ";
            
            _renderer.DrawText(prefix + _options[i], position, color);
        }

        // Draw instructions
        var instructions = "Use UP/DOWN to navigate, LEFT/RIGHT to toggle, ENTER to select";
        var instructionsPosition = new Vector2(400, 500);
        _renderer.DrawText(instructions, instructionsPosition, Color.Gray);
    }

    public void OnActivate()
    {
        _selectedOption = 0;
        UpdateOptionTexts();
        _gameState.InvalidateFrame();
    }

    public void OnDeactivate()
    {
        // No cleanup needed
    }

    private void HandleSelection()
    {
        if (_selectedOption == 3) // Back to Menu
        {
            _gameState.NavigateTo(GameView.Menu);
        }
    }

    private void HandleOptionToggle()
    {
        switch (_selectedOption)
        {
            case 0: // Sound
                _gameState.SetFeedback("Sound toggle not yet implemented");
                break;
            case 1: // Fullscreen
                _gameState.SetFeedback("Fullscreen toggle not yet implemented");
                break;
            case 2: // Game Speed
                CycleGameSpeed();
                break;
        }
    }

    private void CycleGameSpeed()
    {
        _gameState.Speed = _gameState.Speed switch
        {
            GameSpeed.Paused => GameSpeed.Slow,
            GameSpeed.Slow => GameSpeed.Normal,
            GameSpeed.Normal => GameSpeed.Fast,
            GameSpeed.Fast => GameSpeed.Paused,
            _ => GameSpeed.Normal
        };

        UpdateOptionTexts();
        _gameState.SetFeedback($"Game speed set to {_gameState.Speed}");
    }

    private void UpdateOptionTexts()
    {
        _options[2] = $"Game Speed: {_gameState.Speed}";
    }
} 
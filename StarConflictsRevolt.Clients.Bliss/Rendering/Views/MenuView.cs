using System.Numerics;
using StarConflictsRevolt.Clients.Bliss.Core;
using StarConflictsRevolt.Clients.Bliss.Input;

namespace StarConflictsRevolt.Clients.Bliss.Rendering.Views;

/// <summary>
/// Main menu view for the Bliss client.
/// </summary>
public class MenuView : IView
{
    public GameView ViewType => GameView.Menu;

    private readonly GameState _gameState;
    private readonly IInput _input;
    private readonly IRenderer2D _renderer;
    private int _selectedOption = 0;
    private readonly string[] _menuOptions = 
    {
        "Create New Session",
        "Join Existing Session",
        "Galaxy View",
        "Game Options",
        "Exit"
    };

    public MenuView(GameState gameState, IInput input, IRenderer2D renderer)
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
            _selectedOption = (_selectedOption - 1 + _menuOptions.Length) % _menuOptions.Length;
            _gameState.InvalidateFrame();
        }
        else if (_input.IsKeyJustPressed(Key.Down))
        {
            _selectedOption = (_selectedOption + 1) % _menuOptions.Length;
            _gameState.InvalidateFrame();
        }
        else if (_input.IsKeyJustPressed(Key.Enter))
        {
            HandleSelection();
        }
    }

    public void Draw()
    {
        // Draw title
        var title = "Star Conflicts Revolt - Bliss Client";
        var titlePosition = new Vector2(400, 100);
        _renderer.DrawText(title, titlePosition, Color.White, 2.0f);

        // Draw menu options
        var startY = 250;
        var spacing = 50;

        for (int i = 0; i < _menuOptions.Length; i++)
        {
            var position = new Vector2(400, startY + i * spacing);
            var color = i == _selectedOption ? Color.Yellow : Color.White;
            var prefix = i == _selectedOption ? "> " : "  ";
            
            _renderer.DrawText(prefix + _menuOptions[i], position, color);
        }

        // Draw instructions
        var instructions = "Use UP/DOWN arrows to navigate, ENTER to select";
        var instructionsPosition = new Vector2(400, 500);
        _renderer.DrawText(instructions, instructionsPosition, Color.Gray);
    }

    public void OnActivate()
    {
        _selectedOption = 0;
        _gameState.InvalidateFrame();
    }

    public void OnDeactivate()
    {
        // No cleanup needed
    }

    private void HandleSelection()
    {
        switch (_selectedOption)
        {
            case 0: // Create New Session
                _gameState.SetFeedback("Create session functionality not yet implemented");
                break;
            case 1: // Join Existing Session
                _gameState.SetFeedback("Join session functionality not yet implemented");
                break;
            case 2: // Galaxy View
                _gameState.NavigateTo(GameView.Galaxy);
                break;
            case 3: // Game Options
                _gameState.NavigateTo(GameView.GameOptions);
                break;
            case 4: // Exit
                _gameState.ShowConfirmation("Are you sure you want to exit?", () => 
                {
                    // Exit logic would go here
                    _gameState.SetFeedback("Exit functionality not yet implemented");
                });
                break;
        }
    }
} 
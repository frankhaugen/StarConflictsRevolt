using StarConflictsRevolt.Clients.Bliss.Core;
using StarConflictsRevolt.Clients.Bliss.Input;
using StarConflictsRevolt.Clients.Bliss.Rendering;
using Microsoft.Extensions.Logging;
using System.Numerics;

namespace StarConflictsRevolt.Clients.Bliss.Core;

/// <summary>
/// Main game loop implementation with dirty-flag optimization for battery efficiency.
/// </summary>
public class GameLoop : IGameLoop
{
    private readonly ILogger<GameLoop> _logger;
    private readonly object _window; // Mock window
    private readonly IRenderer2D _renderer;
    private readonly IInput _input;
    private readonly IClock _clock;
    private readonly GameState _gameState;
    private readonly IViewFactory _viewFactory;
    private IView? _currentView;
    private bool _shouldClose = false;

    public GameLoop(
        ILogger<GameLoop> logger,
        object window, // Mock window
        IRenderer2D renderer,
        IInput input,
        IClock clock,
        GameState gameState,
        IViewFactory viewFactory)
    {
        _logger = logger;
        _window = window;
        _renderer = renderer;
        _input = input;
        _clock = clock;
        _gameState = gameState;
        _viewFactory = viewFactory;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting Bliss game loop");

        while (!cancellationToken.IsCancellationRequested && !_shouldClose)
        {
            // Update clock
            _clock.Update();

            // Update input
            _input.Update();

            // Handle window events (mock)
            // _window.PumpEvents();

            // Update game logic
            Update(_clock.DeltaTime);

            // Render if frame is invalid (dirty-flag optimization)
            if (_gameState.FrameInvalid)
            {
                Render();
                _gameState.FrameInvalid = false;
            }
            else
            {
                // Skip draw frame for battery efficiency
                await Task.Delay(1, cancellationToken);
            }

            // Cap frame rate
            if (_clock.DeltaTime < 1.0f / 144.0f)
            {
                await Task.Delay(1, cancellationToken);
            }
        }

        _logger.LogInformation("Bliss game loop ended");
    }

    private void Update(float deltaTime)
    {
        // Handle input for navigation
        HandleInput();

        // Update current view
        var view = GetCurrentView();
        view?.Update(deltaTime);
    }

    private void Render()
    {
        // Clear screen
        _renderer.Clear(Color.Black);

        // Begin rendering with camera matrix
        _renderer.Begin(_gameState.CameraMatrix);

        // Draw current view
        var view = GetCurrentView();
        view?.Draw();

        // Draw UI overlays
        DrawUIOverlays();

        // End rendering
        _renderer.End();
    }

    private void HandleInput()
    {
        // Handle navigation keys
        if (_input.IsKeyJustPressed(Key.Escape))
        {
            if (_gameState.ShowConfirmationDialog)
            {
                _gameState.HideConfirmation();
            }
            else
            {
                _gameState.NavigateBack();
            }
        }

        // Handle function keys for view navigation
        if (_input.IsKeyJustPressed(Key.F1))
        {
            _gameState.NavigateTo(GameView.Menu);
        }
        else if (_input.IsKeyJustPressed(Key.F2))
        {
            _gameState.NavigateTo(GameView.FleetFinder);
        }
        else if (_input.IsKeyJustPressed(Key.F3))
        {
            _gameState.NavigateTo(GameView.GameOptions);
        }
        else if (_input.IsKeyJustPressed(Key.F4))
        {
            _gameState.NavigateTo(GameView.PlanetaryFinder);
        }
    }

    private IView? GetCurrentView()
    {
        if (_currentView?.ViewType != _gameState.CurrentView)
        {
            // Deactivate current view
            _currentView?.OnDeactivate();

            // Create new view
            _currentView = _viewFactory.CreateView(_gameState.CurrentView);

            // Activate new view
            _currentView?.OnActivate();
        }

        return _currentView;
    }

    private void DrawUIOverlays()
    {
        // Draw feedback message
        if (!string.IsNullOrEmpty(_gameState.FeedbackMessage) && !_gameState.HasExpiredFeedback)
        {
            _renderer.DrawText(_gameState.FeedbackMessage, new Vector2(10, 10), Color.Yellow);
        }

        // Draw confirmation dialog
        if (_gameState.ShowConfirmationDialog && !string.IsNullOrEmpty(_gameState.ConfirmationMessage))
        {
            DrawConfirmationDialog();
        }
    }

    private void DrawConfirmationDialog()
    {
        // Simple confirmation dialog implementation
        var message = _gameState.ConfirmationMessage!;
        var position = new Vector2(400, 300);
        var color = Color.White;
        
        _renderer.DrawText(message, position, color);
        _renderer.DrawText("Press ENTER to confirm, ESC to cancel", position + new Vector2(0, 30), Color.Gray);
    }
} 
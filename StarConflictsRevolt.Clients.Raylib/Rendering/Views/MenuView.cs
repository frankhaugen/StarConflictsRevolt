using Raylib_CSharp.Colors;
using Raylib_CSharp.Interact;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Windowing;
using StarConflictsRevolt.Clients.Http.Http;
using StarConflictsRevolt.Clients.Models;
using StarConflictsRevolt.Clients.Raylib.Core;
using StarConflictsRevolt.Clients.Raylib.Game.Commands;
using StarConflictsRevolt.Clients.Raylib.Infrastructure.Communication;
using StarConflictsRevolt.Clients.Raylib.Rendering.Core;
using StarConflictsRevolt.Clients.Raylib.Rendering.UI;

namespace StarConflictsRevolt.Clients.Raylib.Rendering.Views;

public class MenuView : IView
{
    private static readonly (string Title, GameView View)[] _views = new[]
    {
        ("Galaxy View", GameView.Galaxy),
        ("Fleet Finder", GameView.FleetFinder),
        ("Game Options", GameView.GameOptions),
        ("Planetary Finder", GameView.PlanetaryFinder),
        ("Troop Finder", GameView.TroopFinder),
        ("Personnel Finder", GameView.PersonnelFinder),
        ("Message Window", GameView.MessageWindow)
    };

    private readonly GameCommandService _commandService;
    private readonly IHttpApiClient _httpApiClient;
    private readonly RenderContext _renderContext;
    private readonly SignalRService _signalRService;
    private List<SessionInfo>? _availableSessions;
    private int _menuState; // 0: main, 1: create single player, 2: create multiplayer, 3: join, 4: player select, 5: session list
    private string _playerName = "";
    private int _selectedSessionIndex;
    private string _selectedSessionType = "Multiplayer";
    private int _selectedView;
    private string _sessionId = "";
    private string _sessionName = "";

    public MenuView(RenderContext renderContext, GameCommandService commandService, SignalRService signalRService, IHttpApiClient httpApiClient)
    {
        _renderContext = renderContext;
        _commandService = commandService;
        _signalRService = signalRService;
        _httpApiClient = httpApiClient;
    }

    public GameView ViewType => GameView.Menu;

    public void Draw()
    {
        Graphics.ClearBackground(UIHelper.Colors.Background);

        // Draw title
        UIHelper.DrawText("Star Conflicts Revolt", 400, 50, UIHelper.FontSizes.Title, Color.White, true);

        // Handle feedback messages
        if (_renderContext.GameState.HasExpiredFeedback) _renderContext.GameState.ClearFeedback();

        var feedbackMessage = _renderContext.GameState.FeedbackMessage;
        if (!string.IsNullOrEmpty(feedbackMessage)) UIHelper.DrawText(feedbackMessage, 400, 100, UIHelper.FontSizes.Medium, UIHelper.Colors.Success, true);

        if (_menuState == 0)
            DrawMainMenu();
        else if (_menuState == 1)
            DrawCreateSinglePlayerSession();
        else if (_menuState == 2)
            DrawCreateMultiplayerSession();
        else if (_menuState == 3)
            DrawJoinSession();
        else if (_menuState == 4)
            DrawPlayerSelect();
        else if (_menuState == 5)
            DrawSessionList();

        // Draw status bar
        var playerName = _renderContext.GameState.PlayerName ?? "Not set";
        var sessionName = _renderContext.GameState.Session?.SessionName ?? "None";
        var sessionType = _renderContext.GameState.Session?.SessionType ?? "None";
        UIHelper.DrawStatusBar(Window.GetScreenHeight() - 30, $"Player: {playerName} | Session: {sessionName} ({sessionType}) | ESC/Backspace: Menu");
    }

    private void DrawMainMenu()
    {
        var centerX = Window.GetScreenWidth() / 2;
        var startY = 150;
        var buttonHeight = 40;
        var buttonSpacing = 50;

        // Main menu buttons
        if (UIHelper.DrawButton("Start Single Player Session", centerX - 150, startY, 300, buttonHeight, UIHelper.Colors.Primary)) _menuState = 1;

        if (UIHelper.DrawButton("Start Multiplayer Session", centerX - 150, startY + buttonSpacing, 300, buttonHeight, UIHelper.Colors.Secondary)) _menuState = 2;

        if (UIHelper.DrawButton("Join Existing Session", centerX - 150, startY + buttonSpacing * 2, 300, buttonHeight)) _menuState = 5;

        if (UIHelper.DrawButton("Exit Game", centerX - 150, startY + buttonSpacing * 3, 300, buttonHeight, UIHelper.Colors.Danger)) Window.Close();

        // View shortcuts
        UIHelper.DrawText("View Shortcuts (F1-F7):", centerX - 100, startY + buttonSpacing * 4 + 20, UIHelper.FontSizes.Small, Color.Gray, true);

        for (var i = 0; i < _views.Length; i++)
        {
            var y = startY + buttonSpacing * 4 + 50 + i * 25;
            UIHelper.DrawText($"F{i + 1}: {_views[i].Title}", centerX - 100, y, UIHelper.FontSizes.Small, Color.LightGray);

            if (Input.IsKeyPressed((KeyboardKey)((int)KeyboardKey.F1 + i)))
            {
                _selectedView = i;
                _renderContext.GameState.NavigateTo(_views[i].View);
                _renderContext.GameState.SetFeedback($"Switched to {_views[i].Title}");
            }
        }
    }

    private void DrawCreateSinglePlayerSession()
    {
        var centerX = Window.GetScreenWidth() / 2;
        var startY = 200;

        UIHelper.DrawText("Create Single Player Session", centerX, startY, UIHelper.FontSizes.Large, Color.White, true);
        UIHelper.DrawText("(AI opponents will be added automatically)", centerX, startY + 30, UIHelper.FontSizes.Small, Color.LightGray, true);

        UIHelper.DrawText("Session Name:", centerX - 100, startY + 70, UIHelper.FontSizes.Medium, Color.White);
        _sessionName = UIHelper.DrawTextInput(_sessionName, centerX - 100, startY + 100, 200, 30, "Enter session name");

        if (UIHelper.DrawButton("Create Single Player Session", centerX - 100, startY + 150, 200, 40, UIHelper.Colors.Success)) _ = CreateSessionAsync("SinglePlayer");

        if (UIHelper.DrawButton("Back", centerX - 100, startY + 200, 200, 40, UIHelper.Colors.Secondary)) _menuState = 0;
    }

    private void DrawCreateMultiplayerSession()
    {
        var centerX = Window.GetScreenWidth() / 2;
        var startY = 200;

        UIHelper.DrawText("Create Multiplayer Session", centerX, startY, UIHelper.FontSizes.Large, Color.White, true);
        UIHelper.DrawText("(No AI opponents - human players only)", centerX, startY + 30, UIHelper.FontSizes.Small, Color.LightGray, true);

        UIHelper.DrawText("Session Name:", centerX - 100, startY + 70, UIHelper.FontSizes.Medium, Color.White);
        _sessionName = UIHelper.DrawTextInput(_sessionName, centerX - 100, startY + 100, 200, 30, "Enter session name");

        if (UIHelper.DrawButton("Create Multiplayer Session", centerX - 100, startY + 150, 200, 40, UIHelper.Colors.Success)) _ = CreateSessionAsync("Multiplayer");

        if (UIHelper.DrawButton("Back", centerX - 100, startY + 200, 200, 40, UIHelper.Colors.Secondary)) _menuState = 0;
    }

    private void DrawSessionList()
    {
        var centerX = Window.GetScreenWidth() / 2;
        var startY = 150;

        UIHelper.DrawText("Available Sessions", centerX, startY, UIHelper.FontSizes.Large, Color.White, true);

        if (_availableSessions == null)
        {
            UIHelper.DrawText("Loading sessions...", centerX, startY + 50, UIHelper.FontSizes.Medium, Color.Yellow, true);
            _ = LoadSessionsAsync();
            return;
        }

        if (!_availableSessions.Any())
        {
            UIHelper.DrawText("No sessions available", centerX, startY + 50, UIHelper.FontSizes.Medium, Color.Gray, true);
            if (UIHelper.DrawButton("Refresh", centerX - 50, startY + 100, 100, 30)) _availableSessions = null;
            if (UIHelper.DrawButton("Back", centerX - 50, startY + 140, 100, 30)) _menuState = 0;
            return;
        }

        // Draw session list
        var listStartY = startY + 50;
        var itemHeight = 60;
        var maxItems = 8;
        var startIndex = Math.Max(0, Math.Min(_selectedSessionIndex - maxItems / 2, _availableSessions.Count - maxItems));

        for (var i = 0; i < Math.Min(maxItems, _availableSessions.Count - startIndex); i++)
        {
            var session = _availableSessions[startIndex + i];
            var y = listStartY + i * itemHeight;
            var isSelected = startIndex + i == _selectedSessionIndex;

            // Highlight selected session
            if (isSelected) Graphics.DrawRectangle(50, y - 5, Window.GetScreenWidth() - 100, itemHeight, UIHelper.Colors.Primary);

            // Session info
            UIHelper.DrawText(session.SessionName, 70, y, UIHelper.FontSizes.Medium, isSelected ? Color.White : Color.LightGray);
            UIHelper.DrawText($"Type: {session.SessionType} | Created: {session.Created:MM/dd HH:mm}", 70, y + 20, UIHelper.FontSizes.Small, Color.Gray);
            UIHelper.DrawText($"Players: {session.PlayerCount} | ID: {session.Id}", 70, y + 35, UIHelper.FontSizes.Small, Color.DarkGray);
        }

        // Navigation
        if (Input.IsKeyPressed(KeyboardKey.Up) && _selectedSessionIndex > 0) _selectedSessionIndex--;
        if (Input.IsKeyPressed(KeyboardKey.Down) && _selectedSessionIndex < _availableSessions.Count - 1) _selectedSessionIndex++;

        // Buttons
        var buttonY = Window.GetScreenHeight() - 100;
        if (UIHelper.DrawButton("Join Selected", centerX - 150, buttonY, 140, 40, UIHelper.Colors.Success))
            if (_selectedSessionIndex >= 0 && _selectedSessionIndex < _availableSessions.Count)
            {
                _sessionId = _availableSessions[_selectedSessionIndex].Id.ToString();
                _sessionName = _availableSessions[_selectedSessionIndex].SessionName;
                _selectedSessionType = _availableSessions[_selectedSessionIndex].SessionType;
                _menuState = 4; // Go to player select
            }

        if (UIHelper.DrawButton("Refresh", centerX, buttonY, 100, 40)) _availableSessions = null;
        if (UIHelper.DrawButton("Back", centerX + 110, buttonY, 100, 40, UIHelper.Colors.Secondary)) _menuState = 0;
    }

    private void DrawJoinSession()
    {
        var centerX = Window.GetScreenWidth() / 2;
        var startY = 200;

        UIHelper.DrawText("Join Existing Session", centerX, startY, UIHelper.FontSizes.Large, Color.White, true);

        UIHelper.DrawText("Session ID:", centerX - 100, startY + 50, UIHelper.FontSizes.Medium, Color.White);
        _sessionId = UIHelper.DrawTextInput(_sessionId, centerX - 100, startY + 80, 200, 30, "Enter session ID");

        if (UIHelper.DrawButton("Continue", centerX - 100, startY + 130, 200, 40, UIHelper.Colors.Success))
            if (!string.IsNullOrWhiteSpace(_sessionId))
                _menuState = 4;

        if (UIHelper.DrawButton("Back", centerX - 100, startY + 180, 200, 40, UIHelper.Colors.Secondary)) _menuState = 0;
    }

    private void DrawPlayerSelect()
    {
        var centerX = Window.GetScreenWidth() / 2;
        var startY = 200;

        UIHelper.DrawText("Enter Player Details", centerX, startY, UIHelper.FontSizes.Large, Color.White, true);

        UIHelper.DrawText("Player Name:", centerX - 100, startY + 50, UIHelper.FontSizes.Medium, Color.White);
        _playerName = UIHelper.DrawTextInput(_playerName, centerX - 100, startY + 80, 200, 30, "Enter player name");

        if (UIHelper.DrawButton("Join Session", centerX - 100, startY + 130, 200, 40, UIHelper.Colors.Success))
            if (!string.IsNullOrWhiteSpace(_playerName))
                _ = JoinSession();

        if (UIHelper.DrawButton("Back", centerX - 100, startY + 180, 200, 40, UIHelper.Colors.Secondary)) _menuState = 3;
    }

    private async Task LoadSessionsAsync()
    {
        _availableSessions = await _httpApiClient.GetSessionsAsync();
        if (_availableSessions == null) _renderContext.GameState.SetFeedback("Failed to load sessions", TimeSpan.FromSeconds(3));
    }

    private async Task CreateSessionAsync(string sessionType)
    {
        if (string.IsNullOrWhiteSpace(_sessionName))
        {
            _renderContext.GameState.SetFeedback("Please enter a session name", TimeSpan.FromSeconds(3));
            return;
        }

        _renderContext.GameState.SetFeedback($"Creating {sessionType} session: {_sessionName}...", TimeSpan.FromSeconds(2));

        var sessionResponse = await _httpApiClient.CreateNewSessionAsync(_sessionName, sessionType);

        if (sessionResponse != null)
        {
            _sessionId = sessionResponse.SessionId.ToString();
            _selectedSessionType = sessionType;

            // Debug logging
            Console.WriteLine($"Session created successfully. SessionId: {sessionResponse.SessionId}, _sessionId: {_sessionId}");

            // Store the world in the ClientWorldStore
            if (sessionResponse.World != null) _renderContext.WorldStore.ApplyFull(sessionResponse.World);
            // Set up GameState for the new session
            _renderContext.GameState.Session = new SessionDto
            {
                Id = sessionResponse.SessionId,
                SessionName = _sessionName,
                IsActive = true,
                SessionType = sessionType
            };
            _menuState = 4;
            _renderContext.GameState.SetFeedback($"{sessionType} session created! Session ID: {_sessionId}. Enter player name.", TimeSpan.FromSeconds(5));
        }
        else
        {
            _renderContext.GameState.SetFeedback("Failed to create session - no response received", TimeSpan.FromSeconds(3));
        }
    }

    private async Task JoinSession()
    {
        Console.WriteLine($"JoinSession called with _sessionId: '{_sessionId}'");

        if (Guid.TryParse(_sessionId, out var sessionGuid))
        {
            Console.WriteLine($"Successfully parsed session GUID: {sessionGuid}");
            try
            {
                _renderContext.GameState.SetFeedback($"Joining session {sessionGuid} as {_playerName}...", TimeSpan.FromSeconds(2));

                // First join the session via HTTP
                var sessionResponse = await _httpApiClient.JoinSessionAsync(sessionGuid, _playerName);

                if (sessionResponse != null)
                {
                    Console.WriteLine($"Successfully joined session {sessionGuid}");

                    // Set up the game state
                    _renderContext.GameState.Session = new SessionDto
                    {
                        Id = sessionResponse.SessionId,
                        SessionName = _sessionName,
                        IsActive = true,
                        SessionType = _selectedSessionType
                    };
                    _renderContext.GameState.PlayerName = _playerName;
                    _renderContext.GameState.PlayerId = Guid.NewGuid().ToString(); // Generate player ID

                    // Apply the world data from the join response
                    if (sessionResponse.World != null) _renderContext.WorldStore.ApplyFull(sessionResponse.World);

                    // Join the SignalR session
                    await _signalRService.JoinSessionAsync(sessionGuid);

                    _renderContext.GameState.NavigateTo(GameView.Galaxy);
                    _renderContext.GameState.SetFeedback($"Joined session as {_playerName}", TimeSpan.FromSeconds(3));
                }
                else
                {
                    _renderContext.GameState.SetFeedback("Failed to join session - no response received", TimeSpan.FromSeconds(3));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception joining session: {ex}");
                _renderContext.GameState.SetFeedback($"Error joining session: {ex.Message}", TimeSpan.FromSeconds(5));
            }
        }
        else
        {
            Console.WriteLine($"Failed to parse session ID: '{_sessionId}'");
            _renderContext.GameState.SetFeedback($"Invalid session ID: {_sessionId}", TimeSpan.FromSeconds(3));
        }
    }
}
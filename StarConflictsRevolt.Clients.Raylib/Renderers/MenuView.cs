using Raylib_CSharp.Colors;
using Raylib_CSharp.Interact;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Windowing;
using StarConflictsRevolt.Clients.Models;
using StarConflictsRevolt.Clients.Raylib.Services;

namespace StarConflictsRevolt.Clients.Raylib.Renderers;

public class MenuView : IView
{
    private readonly RenderContext _renderContext;
    private readonly GameCommandService _commandService;
    private string _sessionName = "";
    private string _sessionId = "";
    private string _playerName = "";
    private int _menuState = 0; // 0: main, 1: create single player, 2: create multiplayer, 3: join, 4: player select
    private int _selectedView = 0;
    private string _selectedSessionType = "Multiplayer";
    private static readonly (string Title, GameView View)[] _views = new[]
    {
        ("Galaxy View", GameView.Galaxy),
        ("Fleet Finder", GameView.FleetFinder),
        ("Game Options", GameView.GameOptions),
        ("Planetary Finder", GameView.PlanetaryFinder),
        ("Troop Finder", GameView.TroopFinder),
        ("Personnel Finder", GameView.PersonnelFinder),
        ("Message Window", GameView.MessageWindow),
    };

    public MenuView(RenderContext renderContext, GameCommandService commandService)
    {
        _renderContext = renderContext;
        _commandService = commandService;
    }

    public GameView ViewType => GameView.Menu;

    public void Draw()
    {
        Graphics.ClearBackground(UIHelper.Colors.Background);
        
        // Draw title
        UIHelper.DrawText("Star Conflicts Revolt", 400, 50, UIHelper.FontSizes.Title, Color.White, true);
        
        // Handle feedback messages
        if (_renderContext.GameState.HasExpiredFeedback)
        {
            _renderContext.GameState.ClearFeedback();
        }
        
        var feedbackMessage = _renderContext.GameState.FeedbackMessage;
        if (!string.IsNullOrEmpty(feedbackMessage))
        {
            UIHelper.DrawText(feedbackMessage, 400, 100, UIHelper.FontSizes.Medium, UIHelper.Colors.Success, true);
        }
        
        if (_menuState == 0)
        {
            DrawMainMenu();
        }
        else if (_menuState == 1)
        {
            DrawCreateSinglePlayerSession();
        }
        else if (_menuState == 2)
        {
            DrawCreateMultiplayerSession();
        }
        else if (_menuState == 3)
        {
            DrawJoinSession();
        }
        else if (_menuState == 4)
        {
            DrawPlayerSelect();
        }
        
        // Draw status bar
        var playerName = _renderContext.GameState.PlayerName ?? "Not set";
        var sessionName = _renderContext.GameState.Session?.SessionName ?? "None";
        var sessionType = _renderContext.GameState.Session?.SessionType ?? "None";
        UIHelper.DrawStatusBar(Window.GetScreenHeight() - 30, $"Player: {playerName} | Session: {sessionName} ({sessionType})");
    }
    
    private void DrawMainMenu()
    {
        var centerX = Window.GetScreenWidth() / 2;
        var startY = 150;
        var buttonHeight = 40;
        var buttonSpacing = 50;
        
        // Main menu buttons
        if (UIHelper.DrawButton("Start Single Player Session", centerX - 150, startY, 300, buttonHeight, UIHelper.Colors.Primary))
        {
            _menuState = 1;
        }
        
        if (UIHelper.DrawButton("Start Multiplayer Session", centerX - 150, startY + buttonSpacing, 300, buttonHeight, UIHelper.Colors.Secondary))
        {
            _menuState = 2;
        }
        
        if (UIHelper.DrawButton("Join Existing Session", centerX - 150, startY + buttonSpacing * 2, 300, buttonHeight))
        {
            _menuState = 3;
        }
        
        if (UIHelper.DrawButton("Exit Game", centerX - 150, startY + buttonSpacing * 3, 300, buttonHeight, UIHelper.Colors.Danger))
        {
            Window.Close();
        }
        
        // View shortcuts
        UIHelper.DrawText("View Shortcuts (F1-F7):", centerX - 100, startY + buttonSpacing * 4 + 20, UIHelper.FontSizes.Small, Color.Gray, true);
        
        for (int i = 0; i < _views.Length; i++)
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
        
        if (UIHelper.DrawButton("Create Single Player Session", centerX - 100, startY + 150, 200, 40, UIHelper.Colors.Success))
        {
            _ = CreateSessionAsync("SinglePlayer");
        }
        
        if (UIHelper.DrawButton("Back", centerX - 100, startY + 200, 200, 40, UIHelper.Colors.Secondary))
        {
            _menuState = 0;
        }
    }
    
    private void DrawCreateMultiplayerSession()
    {
        var centerX = Window.GetScreenWidth() / 2;
        var startY = 200;
        
        UIHelper.DrawText("Create Multiplayer Session", centerX, startY, UIHelper.FontSizes.Large, Color.White, true);
        UIHelper.DrawText("(No AI opponents - human players only)", centerX, startY + 30, UIHelper.FontSizes.Small, Color.LightGray, true);
        
        UIHelper.DrawText("Session Name:", centerX - 100, startY + 70, UIHelper.FontSizes.Medium, Color.White);
        _sessionName = UIHelper.DrawTextInput(_sessionName, centerX - 100, startY + 100, 200, 30, "Enter session name");
        
        if (UIHelper.DrawButton("Create Multiplayer Session", centerX - 100, startY + 150, 200, 40, UIHelper.Colors.Success))
        {
            _ = CreateSessionAsync("Multiplayer");
        }
        
        if (UIHelper.DrawButton("Back", centerX - 100, startY + 200, 200, 40, UIHelper.Colors.Secondary))
        {
            _menuState = 0;
        }
    }
    
    private void DrawJoinSession()
    {
        var centerX = Window.GetScreenWidth() / 2;
        var startY = 200;
        
        UIHelper.DrawText("Join Existing Session", centerX, startY, UIHelper.FontSizes.Large, Color.White, true);
        
        UIHelper.DrawText("Session ID:", centerX - 100, startY + 50, UIHelper.FontSizes.Medium, Color.White);
        _sessionId = UIHelper.DrawTextInput(_sessionId, centerX - 100, startY + 80, 200, 30, "Enter session ID");
        
        if (UIHelper.DrawButton("Continue", centerX - 100, startY + 130, 200, 40, UIHelper.Colors.Success))
        {
            if (!string.IsNullOrWhiteSpace(_sessionId))
            {
                _menuState = 4;
            }
        }
        
        if (UIHelper.DrawButton("Back", centerX - 100, startY + 180, 200, 40, UIHelper.Colors.Secondary))
        {
            _menuState = 0;
        }
    }
    
    private void DrawPlayerSelect()
    {
        var centerX = Window.GetScreenWidth() / 2;
        var startY = 200;
        
        UIHelper.DrawText("Enter Player Details", centerX, startY, UIHelper.FontSizes.Large, Color.White, true);
        
        UIHelper.DrawText("Player Name:", centerX - 100, startY + 50, UIHelper.FontSizes.Medium, Color.White);
        _playerName = UIHelper.DrawTextInput(_playerName, centerX - 100, startY + 80, 200, 30, "Enter player name");
        
        if (UIHelper.DrawButton("Join Session", centerX - 100, startY + 130, 200, 40, UIHelper.Colors.Success))
        {
            if (!string.IsNullOrWhiteSpace(_playerName))
            {
                JoinSession();
            }
        }
        
        if (UIHelper.DrawButton("Back", centerX - 100, startY + 180, 200, 40, UIHelper.Colors.Secondary))
        {
            _menuState = 3;
        }
    }
    
    private async Task CreateSessionAsync(string sessionType)
    {
        if (string.IsNullOrWhiteSpace(_sessionName))
        {
            _renderContext.GameState.SetFeedback("Please enter a session name", TimeSpan.FromSeconds(3));
            return;
        }
        
        var sessionId = await _commandService.CreateSessionAsync(_sessionName, sessionType);
        if (sessionId.HasValue)
        {
            _sessionId = sessionId.Value.ToString();
            _selectedSessionType = sessionType;
            _menuState = 4;
            _renderContext.GameState.SetFeedback($"{sessionType} session created! Enter player name.", TimeSpan.FromSeconds(3));
        }
    }
    
    private void JoinSession()
    {
        if (Guid.TryParse(_sessionId, out var sessionGuid))
        {
            _renderContext.GameState.Session = new SessionDto 
            { 
                Id = sessionGuid, 
                SessionName = _sessionName, 
                IsActive = true,
                SessionType = _selectedSessionType
            };
            _renderContext.GameState.PlayerName = _playerName;
            _renderContext.GameState.PlayerId = Guid.NewGuid().ToString(); // Generate player ID
            _renderContext.GameState.NavigateTo(GameView.Galaxy);
            _renderContext.GameState.SetFeedback($"Joined session as {_playerName}", TimeSpan.FromSeconds(3));
        }
        else
        {
            _renderContext.GameState.SetFeedback("Invalid session ID", TimeSpan.FromSeconds(3));
        }
    }
} 
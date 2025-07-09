using Raylib_CSharp.Colors;
using Raylib_CSharp.Interact;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Windowing;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using StarConflictsRevolt.Clients.Raylib.Renderers;

namespace StarConflictsRevolt.Clients.Raylib.Renderers;

public class MenuView : IView
{
    private readonly RenderContext _renderContext;
    private string _sessionName = "";
    private string _sessionId = "";
    private string _playerName = "";
    private string _feedback = "";
    private int _menuState = 0; // 0: main, 1: create, 2: join, 3: player select
    private int _selectedView = 0;
    private static readonly (string Title, GameView View)[] _views = new[]
    {
        ("Galaxy View", GameView.Galaxy),
        ("Fleet Finder", (GameView)1001), // stub
        ("Game Options", (GameView)1002), // stub
        ("Planetary Finder", (GameView)1003), // stub
        ("Troop Finder", (GameView)1004), // stub
        ("Personnel Finder", (GameView)1005), // stub
        ("Message Window", (GameView)1006), // stub
    };

    public MenuView(RenderContext renderContext)
    {
        _renderContext = renderContext;
    }

    public GameView ViewType => GameView.Menu;

    public void Draw()
    {
        Graphics.ClearBackground(Color.Black);
        Graphics.DrawText("Star Conflicts Revolt", 10, 10, 28, Color.RayWhite);
        Graphics.DrawText($"Current View: {_views[_selectedView].Title}", 10, 40, 20, Color.SkyBlue);
        if (_menuState == 0)
        {
            Graphics.DrawText("1. Create New Session", 10, 60, 20, Color.RayWhite);
            Graphics.DrawText("2. Join Existing Session", 10, 90, 20, Color.RayWhite);
            Graphics.DrawText("3. Exit", 10, 120, 20, Color.RayWhite);
            Graphics.DrawText("F1-F7: Switch Views", 10, 150, 18, Color.LightGray);
            for (int i = 0; i < _views.Length; i++)
            {
                Graphics.DrawText($"F{i + 1}: {_views[i].Title}", 300, 60 + i * 30, 18, Color.LightGray);
                if (Input.IsKeyPressed((KeyboardKey)((int)KeyboardKey.F1 + i)))
                {
                    _selectedView = i;
                    _renderContext.CurrentView = _views[i].View;
                    _feedback = $"Switched to {_views[i].Title}";
                }
            }
            if (Input.IsKeyPressed(KeyboardKey.One)) _menuState = 1;
            if (Input.IsKeyPressed(KeyboardKey.Two)) _menuState = 2;
            if (Input.IsKeyPressed(KeyboardKey.Three) || Input.IsKeyPressed(KeyboardKey.Escape)) Window.Close();
        }
        else if (_menuState == 1)
        {
            Graphics.DrawText("Enter Session Name:", 10, 60, 20, Color.RayWhite);
            Graphics.DrawText(_sessionName + "_", 10, 90, 20, Color.LightGray);
            if (Input.IsKeyPressed(KeyboardKey.Enter) && !string.IsNullOrWhiteSpace(_sessionName))
            {
                _ = CreateSessionAsync(_sessionName);
            }
            else
            {
                var c = Input.GetCharPressed();
                if (c > 0 && char.IsLetterOrDigit((char)c)) _sessionName += (char)c;
                if (Input.IsKeyPressed(KeyboardKey.Backspace) && _sessionName.Length > 0) _sessionName = _sessionName[..^1];
            }
        }
        else if (_menuState == 2)
        {
            Graphics.DrawText("Enter Session ID:", 10, 60, 20, Color.RayWhite);
            Graphics.DrawText(_sessionId + "_", 10, 90, 20, Color.LightGray);
            if (Input.IsKeyPressed(KeyboardKey.Enter) && !string.IsNullOrWhiteSpace(_sessionId))
            {
                _menuState = 3;
            }
            else
            {
                var c = Input.GetCharPressed();
                if (c > 0 && char.IsLetterOrDigit((char)c) || c == '-') _sessionId += (char)c;
                if (Input.IsKeyPressed(KeyboardKey.Backspace) && _sessionId.Length > 0) _sessionId = _sessionId[..^1];
            }
        }
        else if (_menuState == 3)
        {
            Graphics.DrawText("Enter Player Name:", 10, 60, 20, Color.RayWhite);
            Graphics.DrawText(_playerName + "_", 10, 90, 20, Color.LightGray);
            if (Input.IsKeyPressed(KeyboardKey.Enter) && !string.IsNullOrWhiteSpace(_playerName))
            {
                _renderContext.Session = new Clients.Models.SessionDto { Id = Guid.TryParse(_sessionId, out var id) ? id : Guid.NewGuid(), SessionName = _sessionName, IsActive = true };
                _renderContext.ClientId = _playerName;
                _renderContext.CurrentView = GameView.Galaxy;
                _feedback = "Joined session as " + _playerName;
            }
            else
            {
                var c = Input.GetCharPressed();
                if (c > 0 && char.IsLetterOrDigit((char)c)) _playerName += (char)c;
                if (Input.IsKeyPressed(KeyboardKey.Backspace) && _playerName.Length > 0) _playerName = _playerName[..^1];
            }
        }
        if (!string.IsNullOrEmpty(_feedback))
            Graphics.DrawText(_feedback, 10, Window.GetScreenHeight() - 40, 20, Color.Green);
    }

    private async Task CreateSessionAsync(string sessionName)
    {
        try
        {
            using var httpClient = new HttpClient();
            if (!string.IsNullOrEmpty(_renderContext.AccessToken))
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _renderContext.AccessToken);
            var payload = JsonSerializer.Serialize(new { name = sessionName, players = new[] { _renderContext.ClientId ?? "Player" } });
            var content = new StringContent(payload, System.Text.Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync("http://localhost:5267/api/sessions", content);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(json);
                _sessionId = doc.RootElement.GetProperty("sessionId").GetString() ?? "";
                _menuState = 3;
                _feedback = "Session created! Enter player name.";
            }
            else
            {
                _feedback = "Failed to create session.";
            }
        }
        catch (Exception ex)
        {
            _feedback = "Error: " + ex.Message;
        }
    }
} 
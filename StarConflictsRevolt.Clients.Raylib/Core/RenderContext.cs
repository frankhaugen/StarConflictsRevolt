using Microsoft.Extensions.Options;
using StarConflictsRevolt.Clients.Models;
using StarConflictsRevolt.Clients.Raylib.Game.User;
using StarConflictsRevolt.Clients.Raylib.Game.World;
using StarConflictsRevolt.Clients.Raylib.Rendering.Core;

namespace StarConflictsRevolt.Clients.Raylib.Core;

public class RenderContext
{
    private readonly IOptions<GameClientConfiguration> _configurationOptions;
    private readonly ILogger<RenderContext> _logger;
    private readonly UIManager _uiManager;

    public RenderContext(ILogger<RenderContext> logger, IOptions<GameClientConfiguration> configurationOptions, IClientWorldStore worldStore, UIManager uiManager)
    {
        _logger = logger;
        _configurationOptions = configurationOptions;
        WorldStore = worldStore;
        _uiManager = uiManager;
    }

    public GameClientConfiguration Configuration => _configurationOptions.Value;

    public WorldDto? World => WorldStore.GetCurrent();

    // World store access
    public IClientWorldStore WorldStore { get; }

    // Game state management
    public GameState GameState { get; } = new();

    // Legacy properties for backward compatibility
    public SessionDto? Session
    {
        get => GameState.Session;
        set => GameState.Session = value;
    }

    public IGameObject? SelectedObject
    {
        get => GameState.SelectedObject;
        set => GameState.SelectedObject = value;
    }

    public GameView CurrentView
    {
        get => GameState.CurrentView;
        set => GameState.CurrentView = value;
    }

    public string? AccessToken
    {
        get => GameState.AccessToken;
        set => GameState.AccessToken = value;
    }

    public string? ClientId
    {
        get => GameState.PlayerId;
        set => GameState.PlayerId = value;
    }

    public UIManager UIManager => _uiManager;
    public UserProfile UserProfile { get; set; }
}
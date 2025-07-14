using Microsoft.Extensions.Options;
using StarConflictsRevolt.Clients.Models;
using StarConflictsRevolt.Clients.Raylib.Services;

namespace StarConflictsRevolt.Clients.Raylib.Renderers;

public class RenderContext
{
    private readonly IOptions<GameClientConfiguration> _configurationOptions;
    private readonly ILogger<RenderContext> _logger;
    private readonly IClientWorldStore _worldStore;

    public RenderContext(ILogger<RenderContext> logger, IOptions<GameClientConfiguration> configurationOptions, IClientWorldStore worldStore)
    {
        _logger = logger;
        _configurationOptions = configurationOptions;
        _worldStore = worldStore;
    }

    public GameClientConfiguration Configuration => _configurationOptions.Value;

    public WorldDto? World => _worldStore.GetCurrent();

    // World store access
    public IClientWorldStore WorldStore => _worldStore;

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
}
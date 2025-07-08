using Microsoft.Extensions.Options;
using StarConflictsRevolt.Clients.Models;
using StarConflictsRevolt.Clients.Shared;

namespace StarConflictsRevolt.Clients.Raylib;

public class RenderContext
{
    private readonly ILogger<RenderContext> _logger;
    private readonly IOptions<GameClientConfiguration> _configurationOptions;
    private readonly IClientWorldStore _worldStore;

    public RenderContext(ILogger<RenderContext> logger, IOptions<GameClientConfiguration> configurationOptions, IClientWorldStore worldStore)
    {
        _logger = logger;
        _configurationOptions = configurationOptions;
        _worldStore = worldStore;
    }

    public GameClientConfiguration Configuration => _configurationOptions.Value;
    
    public WorldDto? World => _worldStore.GetCurrent();
    
    public SessionDto? Session { get; set; }
    
    public IGameObject? SelectedObject { get; set; }
    public GameView CurrentView { get; set; } = GameView.Menu;
}
using StarConflictsRevolt.Clients.Raylib.Core;
using StarConflictsRevolt.Clients.Raylib.Infrastructure.Communication;
using StarConflictsRevolt.Clients.Raylib.Rendering.Core;

namespace StarConflictsRevolt.Clients.Raylib.Infrastructure.Services;

public class ClientServiceHost : BackgroundService
{
    private readonly IGameRenderer _gameRenderer;
    private readonly ILogger<ClientServiceHost> _logger;
    private readonly RenderContext _renderContext;
    private readonly SignalRService _signalRService;

    public ClientServiceHost(SignalRService renderService, RenderContext renderContext, IGameRenderer gameRenderer, ILogger<ClientServiceHost> logger)
    {
        _signalRService = renderService;
        _renderContext = renderContext;
        _gameRenderer = gameRenderer;
        _logger = logger;
        _logger.LogInformation("ClientServiceHost initialized");
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting ClientServiceHost");
        
        _logger.LogInformation("Initializing User Profile");
        _renderContext.GameState.PlayerName = _renderContext.UserProfile?.DisplayName ?? "Unknown Player";
        _logger.LogInformation("User Profile initialized: {PlayerName}", _renderContext.GameState.PlayerName);

        _logger.LogInformation("Setting initial view to Menu");
        _renderContext.CurrentView = GameView.Menu; // Set initial view to Menu

        _logger.LogInformation("Starting game renderer");
        await _gameRenderer.RenderAsync(null, stoppingToken);

        _logger.LogInformation("Starting SignalR service");
        await _signalRService.StartAsync(stoppingToken);

        _logger.LogInformation("ClientServiceHost started successfully");
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping ClientServiceHost");
        await _signalRService.StopAsync();
        _logger.LogInformation("ClientServiceHost stopped");
    }
}
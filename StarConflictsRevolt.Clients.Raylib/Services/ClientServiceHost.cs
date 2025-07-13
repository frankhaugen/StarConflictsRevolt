using StarConflictsRevolt.Clients.Raylib.Renderers;

namespace StarConflictsRevolt.Clients.Raylib.Services;

public class ClientServiceHost : BackgroundService
{
    private readonly SignalRService _signalRService;
    private readonly RenderContext _renderContext;
    private readonly IGameRenderer _gameRenderer;
    private readonly ILogger<ClientServiceHost> _logger;
    
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
using StarConflictsRevolt.Clients.Raylib.Http;
using StarConflictsRevolt.Clients.Raylib.Renderers;

namespace StarConflictsRevolt.Clients.Raylib.Services;

public class ClientServiceHost : IHostedService
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

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting ClientServiceHost");
        
        _logger.LogInformation("Setting initial view to Menu");
        _renderContext.CurrentView = GameView.Menu; // Set initial view to Menu
        
        _logger.LogInformation("Starting game renderer");
        await _gameRenderer.RenderAsync(null, cancellationToken);
        
        _logger.LogInformation("Starting SignalR service");
        await _signalRService.StartAsync(cancellationToken);
        
        _logger.LogInformation("ClientServiceHost started successfully");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping ClientServiceHost");
        await _signalRService.StopAsync();
        _logger.LogInformation("ClientServiceHost stopped");
    }
}
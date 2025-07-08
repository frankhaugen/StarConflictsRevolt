using StarConflictsRevolt.Clients.Shared;

namespace StarConflictsRevolt.Clients.Raylib;

public class ClientServiceHost : IHostedService
{
    private readonly SignalRService _signalRService;
    private readonly RenderContext _renderContext;
    private readonly IGameRenderer _gameRenderer;
    
    public ClientServiceHost(SignalRService renderService, RenderContext renderContext, IGameRenderer gameRenderer)
    {
        _signalRService = renderService;
        _renderContext = renderContext;
        _gameRenderer = gameRenderer;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _renderContext.CurrentView = GameView.Menu; // Set initial view to Menu
        await _gameRenderer.RenderAsync(null, cancellationToken);
        await _signalRService.StartAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => _signalRService.StopAsync();
}
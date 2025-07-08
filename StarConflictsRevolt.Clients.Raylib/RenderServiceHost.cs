namespace StarConflictsRevolt.Clients.Raylib;

public class RenderServiceHost : IHostedService
{
    private readonly RenderService _renderService;
    public RenderServiceHost(RenderService renderService) => _renderService = renderService;

    public Task StartAsync(CancellationToken cancellationToken) => _renderService.StartAsync(cancellationToken);

    public Task StopAsync(CancellationToken cancellationToken) => _renderService.StopAsync();
}
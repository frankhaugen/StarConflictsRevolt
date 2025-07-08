namespace StarConflictsRevolt.Clients.Raylib;

public class RenderServiceHost(RenderService renderService) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken) => renderService.StartAsync(cancellationToken);

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
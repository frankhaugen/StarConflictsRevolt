using StarConflictsRevolt.Clients.Models;
using StarConflictsRevolt.Clients.Shared.Communication;

namespace StarConflictsRevolt.Clients.Blazor.Services;

public class BlazorSignalRService : ISignalRService
{
    private readonly ISignalRService _baseSignalRService;

    public BlazorSignalRService(ISignalRService baseSignalRService)
    {
        _baseSignalRService = baseSignalRService;
    }

    public event Action<WorldDto>? FullWorldReceived
    {
        add => _baseSignalRService.FullWorldReceived += value;
        remove => _baseSignalRService.FullWorldReceived -= value;
    }

    public event Action<List<GameObjectUpdate>>? UpdatesReceived
    {
        add => _baseSignalRService.UpdatesReceived += value;
        remove => _baseSignalRService.UpdatesReceived -= value;
    }

    public event Action<Exception?>? ConnectionClosed
    {
        add => _baseSignalRService.ConnectionClosed += value;
        remove => _baseSignalRService.ConnectionClosed -= value;
    }

    public event Action<Exception?>? Reconnecting
    {
        add => _baseSignalRService.Reconnecting += value;
        remove => _baseSignalRService.Reconnecting -= value;
    }

    public event Action<string>? Reconnected
    {
        add => _baseSignalRService.Reconnected += value;
        remove => _baseSignalRService.Reconnected -= value;
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        await _baseSignalRService.StartAsync(cancellationToken);
    }

    public async Task StopAsync()
    {
        await _baseSignalRService.StopAsync();
    }

    public async Task JoinSessionAsync(Guid sessionId)
    {
        await _baseSignalRService.JoinSessionAsync(sessionId);
    }

    public async ValueTask DisposeAsync()
    {
        await _baseSignalRService.DisposeAsync();
    }
}

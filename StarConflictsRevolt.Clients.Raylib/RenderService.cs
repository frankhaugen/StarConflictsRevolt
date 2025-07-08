using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;
using StarConflictsRevolt.Clients.Models;
using StarConflictsRevolt.Clients.Shared;

namespace StarConflictsRevolt.Clients.Raylib;

public class RenderService
{
    private HubConnection? _hubConnection;
    private readonly IOptions<GameClientConfiguration> _gameClientConfiguration;
    private IClientWorldStore _worldStore;
    private IGameRenderer _gameRenderer;

    public RenderService(IOptions<GameClientConfiguration> gameClientConfiguration, IClientWorldStore worldStore, IGameRenderer gameRenderer)
    {
        _gameClientConfiguration = gameClientConfiguration;
        _worldStore = worldStore;
        _gameRenderer = gameRenderer;
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        
        _hubConnection = new HubConnectionBuilder()
            .WithUrl(_gameClientConfiguration.Value.GameServerHubUrl)
            .WithAutomaticReconnect()
            .ConfigureLogging(logging =>
            {
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Information);
            })
            .Build();
        
        _hubConnection.On<WorldDto>("FullWorld", worldDto =>
        {
            _worldStore.ApplyFull(worldDto);
            var world = _worldStore.GetCurrent();
            _gameRenderer.RenderAsync(world, CancellationToken.None);
        });
        
        _hubConnection.On<List<GameObjectUpdate>>("ReceiveUpdates", updates =>
        {
            _worldStore.ApplyDeltas(updates);
            var world = _worldStore.GetCurrent();
            _gameRenderer.RenderAsync(world, CancellationToken.None);
        });

        try
        {
            await _hubConnection.StartAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error connecting to game hub: {ex.Message}");
            // Handle connection error
        }

        if (_hubConnection != null)
            await _hubConnection.DisposeAsync();
    }

}
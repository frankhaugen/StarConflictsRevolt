using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StarConflictsRevolt.Clients.Blazor;
using StarConflictsRevolt.Clients.Shared;
using Microsoft.AspNetCore.Builder;
using StarConflictsRevolt.Clients.Blazor.Services;
using StarConflictsRevolt.Clients.Models;
using StarConflictsRevolt.Clients.Shared.Communication;
using StarConflictsRevolt.Clients.Blazor.Components;

namespace StarConflictsRevolt.Tests.ClientTests.UITests.TestHost;

/// <summary>
/// Test host for running the Blazor application during UI tests
/// </summary>
public class BlazorTestHost : IDisposable
{
    private WebApplication? _host;
    private bool _disposed = false;
    
    public int Port { get; private set; }
    public string BaseUrl => $"http://localhost:{Port}";
    
    public async Task StartAsync()
    {
        // Find an available port
        Port = await FindAvailablePortAsync();
        
        var builder = WebApplication.CreateBuilder();
        
        // Configure Kestrel with HTTP only for testing (simpler setup)
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.ListenLocalhost(Port);
        });
        
        // Configure services
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();
            
        // Add test configuration
        builder.Services.Configure<GameClientConfiguration>(config =>
        {
            config.GameServerUrl = "https://localhost:7002";
            config.GameServerHubUrl = "https://localhost:7002/gamehub";
            config.ApiBaseUrl = "https://localhost:7002";
        });
        
        // Add mock services for testing
        builder.Services.AddSingleton<ISignalRService, MockSignalRService>();
        builder.Services.AddScoped<IGameStateService, MockGameStateService>();
        builder.Services.AddScoped<BlazorSignalRService>();
        builder.Services.AddSingleton<TelemetryService>();
        
        // Configure logging
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Logging.SetMinimumLevel(LogLevel.Warning);
        
        var app = builder.Build();
        
        // Configure pipeline
        app.UseAntiforgery();
        app.UseStaticFiles();
        
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();
            
        _host = app;
        
        Console.WriteLine($"Starting test host on port {Port}...");
        await _host.StartAsync();
        Console.WriteLine($"Test host started successfully on {BaseUrl}");
    }
    
    public async Task StopAsync()
    {
        if (_host != null)
        {
            await _host.StopAsync();
            await _host.DisposeAsync();
            _host = null;
        }
    }
    
    private static async Task<int> FindAvailablePortAsync()
    {
        // Use system-assigned port for reliability
        var listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 0);
        listener.Start();
        var port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }
    
    public void Dispose()
    {
        if (!_disposed)
        {
            StopAsync().GetAwaiter().GetResult();
            _disposed = true;
        }
    }
}


/// <summary>
/// Mock SignalR service for testing
/// </summary>
public class MockSignalRService : ISignalRService
{
    public event Action<WorldDto>? FullWorldReceived;
    public event Action<List<GameObjectUpdate>>? UpdatesReceived;
    public event Action<Exception?>? ConnectionClosed;
    public event Action<Exception?>? Reconnecting;
    public event Action<string>? Reconnected;
    
    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
    
    public Task StopAsync()
    {
        return Task.CompletedTask;
    }
    
    public Task JoinSessionAsync(Guid sessionId)
    {
        return Task.CompletedTask;
    }
    
    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
    
    public void Dispose()
    {
    }
}

/// <summary>
/// Mock game state service for testing
/// </summary>
public class MockGameStateService : IGameStateService
{
    public WorldDto? CurrentWorld { get; private set; }
    public SessionDto? CurrentSession { get; private set; }
    public bool IsConnected => true;
    
    public event Action? StateChanged;
    
    public Task<bool> CreateSessionAsync(string sessionName)
    {
        CurrentSession = new SessionDto
        {
            Id = Guid.NewGuid(),
            SessionName = sessionName,
            SessionType = "SinglePlayer",
            Created = DateTime.UtcNow,
            IsActive = true
        };
        
        CurrentWorld = new WorldDto(
            Guid.NewGuid(),
            new GalaxyDto(
                Guid.NewGuid(),
                new List<StarSystemDto>
                {
                    new StarSystemDto(
                        Guid.NewGuid(),
                        "Test System",
                        new List<PlanetDto>(),
                        new System.Numerics.Vector2(0, 0)
                    )
                }
            ),
            null
        );
        
        StateChanged?.Invoke();
        return Task.FromResult(true);
    }
    
    public Task<bool> JoinSessionAsync(Guid sessionId)
    {
        CurrentSession = new SessionDto
        {
            Id = sessionId,
            SessionName = "Test Session",
            SessionType = "Multiplayer",
            Created = DateTime.UtcNow,
            IsActive = true
        };
        
        StateChanged?.Invoke();
        return Task.FromResult(true);
    }
    
    public Task<bool> LeaveSessionAsync()
    {
        CurrentSession = null;
        CurrentWorld = null;
        StateChanged?.Invoke();
        return Task.FromResult(true);
    }
    
    public Task<List<SessionDto>> GetAvailableSessionsAsync()
    {
        return Task.FromResult(new List<SessionDto>());
    }
    
    public Task<bool> MoveFleetAsync(Guid fleetId, Guid fromPlanetId, Guid toPlanetId)
    {
        return Task.FromResult(true);
    }
    
    public Task<bool> BuildStructureAsync(Guid planetId, string structureType)
    {
        return Task.FromResult(true);
    }
    
    public Task<bool> AttackAsync(Guid attackerFleetId, Guid targetFleetId)
    {
        return Task.FromResult(true);
    }
}


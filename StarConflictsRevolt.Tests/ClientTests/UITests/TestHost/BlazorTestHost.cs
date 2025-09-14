using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StarConflictsRevolt.Clients.Blazor;
using StarConflictsRevolt.Clients.Shared;

namespace StarConflictsRevolt.Tests.ClientTests.UITests.TestHost;

/// <summary>
/// Test host for running the Blazor application during UI tests
/// </summary>
public class BlazorTestHost : IDisposable
{
    private IHost? _host;
    private bool _disposed = false;
    
    public int Port { get; private set; }
    public string BaseUrl => $"https://localhost:{Port}";
    
    public async Task StartAsync()
    {
        // Find an available port
        Port = await FindAvailablePortAsync();
        
        var builder = Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<TestStartup>();
                webBuilder.UseUrls($"https://localhost:{Port}");
                webBuilder.UseKestrel(options =>
                {
                    options.ConfigureHttpsDefaults(httpsOptions =>
                    {
                        httpsOptions.ServerCertificate = TestCertificateProvider.GetCertificate();
                    });
                });
            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Warning);
            });
            
        _host = builder.Build();
        await _host.StartAsync();
    }
    
    public async Task StopAsync()
    {
        if (_host != null)
        {
            await _host.StopAsync();
            _host.Dispose();
            _host = null;
        }
    }
    
    private static async Task<int> FindAvailablePortAsync()
    {
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
/// Test startup configuration for the Blazor app
/// </summary>
public class TestStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Add minimal services needed for testing
        services.AddRazorComponents()
            .AddInteractiveServerComponents();
            
        // Add test configuration
        services.Configure<GameClientConfiguration>(config =>
        {
            config.GameServerUrl = "https://localhost:7002";
            config.GameServerHubUrl = "https://localhost:7002/gamehub";
            config.ApiBaseUrl = "https://localhost:7002";
        });
        
        // Add mock services for testing
        services.AddSingleton<ISignalRService, MockSignalRService>();
        services.AddScoped<IGameStateService, MockGameStateService>();
        services.AddScoped<BlazorSignalRService>();
    }
    
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        
        app.UseHttpsRedirection();
        app.UseAntiforgery();
        app.UseStaticFiles();
        
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();
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
        
        CurrentWorld = new WorldDto
        {
            Galaxy = new GalaxyDto
            {
                StarSystems = new List<StarSystemDto>
                {
                    new StarSystemDto
                    {
                        Id = Guid.NewGuid(),
                        Name = "Test System",
                        Coordinates = "0,0",
                        Planets = new List<PlanetDto>()
                    }
                }
            }
        };
        
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

/// <summary>
/// Test certificate provider for HTTPS in tests
/// </summary>
public static class TestCertificateProvider
{
    public static System.Security.Cryptography.X509Certificates.X509Certificate2 GetCertificate()
    {
        // In a real implementation, you would generate or load a test certificate
        // For now, we'll use a self-signed certificate
        return new System.Security.Cryptography.X509Certificates.X509Certificate2();
    }
}

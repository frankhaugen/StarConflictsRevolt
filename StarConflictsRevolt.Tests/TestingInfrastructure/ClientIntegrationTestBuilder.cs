using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StarConflictsRevolt.Clients.Http.Authentication;
using StarConflictsRevolt.Clients.Http.Configuration;
using StarConflictsRevolt.Clients.Http.Http;
using StarConflictsRevolt.Clients.Raylib.Renderers;
using StarConflictsRevolt.Clients.Raylib.Services;

namespace StarConflictsRevolt.Tests.TestingInfrastructure;

public class ClientIntegrationTestBuilder : IDisposable
{
    private readonly FullIntegrationTestWebApplicationBuilder _serverBuilder;
    private readonly IHost _clientHost;
    private readonly TestViewFactory _testViewFactory;
    private readonly TestGameRenderer _testGameRenderer;
    private readonly TestClientWorldStore _testWorldStore;
    private readonly TestSignalRService _testSignalRService;

    public ClientIntegrationTestBuilder()
    {
        // Build the server first
        _serverBuilder = new FullIntegrationTestWebApplicationBuilder();
        var server = _serverBuilder.WebApplication;
        
        // Ensure database is created
        using var scope = server.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<StarConflictsRevolt.Server.WebApi.Datastore.GameDbContext>();
        dbContext.Database.EnsureCreated();
        
        // Start the server
        server.Start();

        // Build the client with test implementations
        var clientBuilder = Host.CreateApplicationBuilder();
        
        // Add HTTP client factory and related services
        clientBuilder.Services.AddHttpClient();
        
        // Add token provider services (simplified for testing)
        clientBuilder.Services.AddSingleton<ITokenProvider, TestTokenProvider>();
        clientBuilder.Services.AddTransient<JwtTokenHandler>();
        clientBuilder.Services.Configure<TokenProviderOptions>(options =>
        {
            options.TokenEndpoint = $"http://localhost:{_serverBuilder.GetPort()}/token";
            options.ClientId = "test-client";
            options.Secret = "test-secret";
        });
        
        // Add minimal client services
        clientBuilder.Services.AddLogging(logging => logging.AddConsole());
        clientBuilder.Services.AddSingleton<IClientWorldStore, TestClientWorldStore>();
        clientBuilder.Services.AddSingleton<IGameRenderer, TestGameRenderer>();
        clientBuilder.Services.AddSingleton<IViewFactory, TestViewFactory>();
        clientBuilder.Services.AddSingleton<RenderContext>();
        clientBuilder.Services.AddSingleton<GameState>();
        clientBuilder.Services.AddSingleton<IClientIdentityService, TestClientIdentityService>();
        clientBuilder.Services.AddSingleton<IClientInitializer, TestClientInitializer>();

        // Register HttpApiClient with test configuration
        clientBuilder.Services.AddTransient(sp => new HttpApiClient(sp.GetRequiredService<IHttpClientFactory>(), "GameApi"));
        
        // Register GameCommandService after its dependencies
        clientBuilder.Services.AddSingleton<GameCommandService>();

        // Register test views
        clientBuilder.Services.AddSingleton<IView, TestMenuView>();
        clientBuilder.Services.AddSingleton<IView, TestGalaxyView>();

        // Configure client to connect to test server
        clientBuilder.Configuration["GameClientConfiguration:ApiBaseUrl"] = $"http://localhost:{_serverBuilder.GetPort()}";
        clientBuilder.Configuration["GameClientConfiguration:GameServerHubUrl"] = _serverBuilder.GetGameServerHubUrl();
        clientBuilder.Configuration["TokenProviderOptions:TokenEndpoint"] = $"http://localhost:{_serverBuilder.GetPort()}/token";
        clientBuilder.Configuration["TokenProviderOptions:ClientId"] = "test-client";
        clientBuilder.Configuration["TokenProviderOptions:Secret"] = "test-secret";

        _clientHost = clientBuilder.Build();

        // Get test implementations for assertions
        _testViewFactory = (TestViewFactory)_clientHost.Services.GetRequiredService<IViewFactory>();
        _testGameRenderer = (TestGameRenderer)_clientHost.Services.GetRequiredService<IGameRenderer>();
        _testWorldStore = (TestClientWorldStore)_clientHost.Services.GetRequiredService<IClientWorldStore>();
        _testSignalRService = new TestSignalRService(); // We'll inject this later if needed
    }

    public FullIntegrationTestWebApplicationBuilder ServerBuilder => _serverBuilder;
    public IHost ClientHost => _clientHost;
    public TestViewFactory TestViewFactory => _testViewFactory;
    public TestGameRenderer TestGameRenderer => _testGameRenderer;
    public TestClientWorldStore TestWorldStore => _testWorldStore;
    public TestSignalRService TestSignalRService => _testSignalRService;

    public async Task InitializeClientAsync()
    {
        var initializer = _clientHost.Services.GetRequiredService<IClientInitializer>();
        await initializer.InitializeAsync();
    }

    public async Task StartClientAsync()
    {
        await _clientHost.StartAsync();
    }

    public async Task StopAsync()
    {
        await _clientHost.StopAsync();
        await _serverBuilder.WebApplication.StopAsync();
    }

    public void Dispose()
    {
        _clientHost?.Dispose();
        _serverBuilder?.Dispose();
    }
}
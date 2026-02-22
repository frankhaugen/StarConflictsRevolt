using System.Net;
using System.Net.Sockets;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using StarConflictsRevolt.Clients.Models;
using StarConflictsRevolt.Server.WebApi.API.Handlers.Endpoints;
using StarConflictsRevolt.Server.Application.Services.Gameplay;
using StarConflictsRevolt.Server.AI;
using StarConflictsRevolt.Server.Domain.AI;
using StarConflictsRevolt.Server.Domain.Engine;
using StarConflictsRevolt.Server.Simulation.Engine;
using StarConflictsRevolt.Server.EventStorage.Abstractions;
using StarConflictsRevolt.Server.EventStorage.RavenDB;
using StarConflictsRevolt.Server.WebApi.Infrastructure.Configuration;
using StarConflictsRevolt.Server.WebApi.Infrastructure.Security;
using StarConflictsRevolt.Server.WebApi.Infrastructure.Transport;
using StarConflictsRevolt.Clients.Blazor.Services;
using StarConflictsRevolt.Clients.Shared.Authentication;
using StarConflictsRevolt.Clients.Shared.Communication;
using StarConflictsRevolt.Clients.Shared.Configuration;
using StarConflictsRevolt.Clients.Shared.Http;
using StarConflictsRevolt.Clients.Shared.Infrastructure;
using StarConflictsRevolt.Clients.Shared.User;
using ServiceCollectionExtensions = StarConflictsRevolt.Clients.Shared.Authentication.ServiceCollectionExtensions;

namespace StarConflictsRevolt.Tests.TestingInfrastructure;

public class TestHostApplication : IDisposable
{
    private readonly IDocumentStore _documentStore;
    private readonly string _uniqueDataDir;

    public TestHostApplication(bool includeClientServices = true)
    {
        Port = FindRandomUnusedPort();
        _uniqueDataDir = Path.Combine(Path.GetTempPath(), $"StarConflictsRevoltTest_{Guid.NewGuid()}");

        var builder = WebApplication.CreateBuilder();

        // Add client services
        // Configure HTTP client with the new standardized library
        ServiceCollectionExtensions.AddStarConflictsHttpClients(builder.Services, builder.Configuration, "GameApi", client => { client.BaseAddress = new Uri("http://127.0.0.1:" + Port); });

        if (includeClientServices)
        {
            // Bind configuration
            builder.Services.Configure<GameClientConfiguration>(
                builder.Configuration.GetSection("GameClientConfiguration"));

            // Register infrastructure services
            builder.Services.AddSingleton<SignalRService>();

            // Register client initialization services
            builder.Services.AddSingleton<IClientIdentityService, ClientIdentityService>();
            builder.Services.AddSingleton<IClientInitializer, ClientInitializer>();
            builder.Services.AddSingleton<IUserProfileProvider, UserProfileProvider>();

            // Add client services
            builder.Services.AddSingleton<IClientWorldStore, ClientWorldStore>();
            builder.Services.AddSingleton<IClientContext, ClientContext>();
            
        }

        // Use the same shared document store as RavenDbDataSourceAttribute
        _documentStore = SharedDocumentStore.CreateStore("test-database-" + Guid.NewGuid().ToString("N"));
        builder.Services.AddSingleton(_documentStore);
        builder.Services.AddScoped<IAsyncDocumentSession>(sp =>
            sp.GetRequiredService<IDocumentStore>().OpenAsyncSession());

        AddInMemoryConfigurationOverrides(builder);
        StartupHelper.RegisterLiteDb(builder);

        // Register test-specific services instead of production StartupHelper
        builder.Services.AddSingleton<IEventStore, RavenEventStore>();
        builder.Services.AddSingleton<CommandQueue>();
        builder.Services.AddSingleton<SessionAggregateManager>();
        builder.Services.AddSingleton<WorldFactory>();
        builder.Services.AddSingleton<ICommandQueue, CommandQueueChannel>();
        builder.Services.AddSingleton<IGameSim, GameSim>();
        builder.Services.AddSingleton<WorldEngine>();

        // Simulation manager (ticker speed)
        builder.Services.AddSingleton<ISimulationManager, SimulationManager>();

        // Transport: tick fan-out to in-process listeners and SignalR
        builder.Services.AddSingleton<ITickListener, AiTurnTickListener>();
        builder.Services.AddSingleton<ITickListener, GameUpdateTickListener>();
        builder.Services.AddSingleton<TickTransport>();
        builder.Services.AddSingleton<ITickPublisher>(sp => sp.GetRequiredService<TickTransport>());

        builder.Services.AddScoped<SessionService>();
        builder.Services.AddScoped<WorldService>();
        builder.Services.AddScoped<LeaderboardService>();

        builder.Services.AddSingleton<IAiStrategy, DefaultAiStrategy>();
        builder.Services.AddSingleton<AiMemoryBank>();
        builder.Services.AddSingleton<AiTurnService>();
        builder.Services.AddSingleton<GameUpdateService>();

        // Add SignalR
        builder.Services.AddSignalR(config =>
        {
            config.EnableDetailedErrors = true;
            config.MaximumReceiveMessageSize = 1024 * 1024; // 1 MB
        });

        // Add CORS
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });

        // Add OpenAPI
        builder.Services.AddOpenApi();

        // Add JWT authentication matching standard "Bearer" scheme
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = false,
                    RequireExpirationTime = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = JwtConfig.GetSymmetricSecurityKey()
                };
            });

        // Add API versioning
        builder.Services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
        });

        // Add hosted services for full application testing
        // These services are disabled by default to prevent hangs in simple tests
        // They should only be enabled for tests that specifically need the full game loop
        if (includeClientServices)
        {
            builder.Services.AddHostedService<GameTickService>();
            builder.Services.AddHostedService<ProjectionService>();
            builder.Services.AddHostedService<EventBroadcastService>();
        }


        // Configure the web application
        builder.WebHost.UseUrls($"http://localhost:{Port}");

        Server = builder.Build();
        Server.Urls.Add($"http://localhost:{Port}");

        // Configure the HTTP request pipeline for tests
        ConfigureTestApplication(Server);

        // Create the API client
        Client = new HttpApiClient(Server.Services.GetRequiredService<IHttpClientFactory>(), Constants.HttpClientName);
    }

    // Public properties for test access
    public WebApplication Server { get; }

    public WebApplication App => Server;
    public IHttpApiClient Client { get; }

    public IDocumentStore DocumentStore => _documentStore ?? throw new InvalidOperationException("DocumentStore not initialized");

    public int Port { get; }

    public void Dispose()
    {
        if (Server is IAsyncDisposable asyncApp)
            asyncApp.DisposeAsync().AsTask().GetAwaiter().GetResult();
        else
            (Server as IDisposable)?.Dispose();

        try
        {
            if (Directory.Exists(_uniqueDataDir))
                Directory.Delete(_uniqueDataDir, true);
        }
        catch
        {
            /* ignore cleanup errors */
        }
    }

    private void AddInMemoryConfigurationOverrides(WebApplicationBuilder builder)
    {
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string>
        {
            ["ConnectionStrings:liteDb"] = "Filename=:memory:",
            ["ConnectionStrings:ravenDb"] = "http://localhost:8080",
            ["TokenProviderOptions:TokenEndpoint"] = "http://localhost:" + Port + "/token",
            ["TokenProviderOptions:ClientId"] = "test-client",
            ["TokenProviderOptions:Secret"] = Constants.Secret,
            ["GameClientConfiguration:GameServerHubUrl"] = "http://localhost:" + Port + "/gamehub",
            ["GameClientConfiguration:GameApiBaseUrl"] = "http://localhost:" + Port,
            ["GameClientConfiguration:DefaultSessionName"] = "Test Session",
            ["GameClientConfiguration:DefaultSessionType"] = "Multiplayer"
        }!);
    }

    private void ConfigureTestApplication(WebApplication app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
        ApiEndpointHandler.MapAllEndpoints(app);
        app.MapOpenApi();
        app.MapHub<WorldHub>("/gamehub");
        app.UseCors();

        // LiteDB is ready on first use; no EnsureCreated needed.
    }

    private static int FindRandomUnusedPort()
    {
        try
        {
            using var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }
        catch (Exception)
        {
            // Fallback to a random port in a safe range
            return Random.Shared.Next(49152, 65535);
        }
    }

    public async Task UseGamePersistenceAsync(Func<IGamePersistence, Task> action)
    {
        await using var scope = Server.Services.CreateAsyncScope();
        var persistence = scope.ServiceProvider.GetRequiredService<IGamePersistence>();
        await action(persistence);
    }

    public string GetGameServerHubUrl()
    {
        return $"http://localhost:{Port}/gamehub";
    }

    public string GetPortAsString()
    {
        return Port.ToString();
    }

    public async Task StartServerAsync(CancellationToken cancellationToken)
    {
        if (Server == null)
            throw new InvalidOperationException("Application is not initialized. Call the constructor first.");

        // Start the application if not already started
        if (Server.Lifetime.ApplicationStarted.IsCancellationRequested) return; // Already started

        // Ensure the application is started
        await Server.StartAsync(cancellationToken);
    }

    public HttpClient GetHttpClient()
    {
        if (Server == null)
            throw new InvalidOperationException("Application is not initialized. Call the constructor first.");
        if (Server.Services == null)
            throw new InvalidOperationException("Application services are not available. Ensure the application is started.");

        return Server.Services.GetRequiredService<IHttpClientFactory>().CreateClient(Constants.HttpClientName);
    }
}
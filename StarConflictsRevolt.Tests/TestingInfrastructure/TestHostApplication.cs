using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using StarConflictsRevolt.Clients.Http;
using StarConflictsRevolt.Clients.Http.Http;
using StarConflictsRevolt.Clients.Models;
using StarConflictsRevolt.Clients.Raylib.Renderers;
using StarConflictsRevolt.Clients.Raylib.Services;
using StarConflictsRevolt.Server.WebApi;
using StarConflictsRevolt.Server.WebApi.Datastore;
using StarConflictsRevolt.Server.WebApi.Eventing;
using StarConflictsRevolt.Server.WebApi.Helpers;
using StarConflictsRevolt.Server.WebApi.Services;
using StarConflictsRevolt.Tests.TestingInfrastructure.TestViews;

namespace StarConflictsRevolt.Tests.TestingInfrastructure;

public class TestHostApplication : IDisposable
{
    private readonly WebApplication _app;
    private readonly SqliteConnection _sqliteConnection;
    private readonly int _port;
    private readonly string _uniqueDataDir;
    private readonly IHttpApiClient _apiClient;
    private readonly IDocumentStore _documentStore;

    public TestHostApplication(bool includeClientServices = true)
    {
        _port = FindRandomUnusedPort();
        _uniqueDataDir = Path.Combine(Path.GetTempPath(), $"StarConflictsRevoltTest_{Guid.NewGuid()}");
        _sqliteConnection = new SqliteConnection("DataSource=:memory:");
        
        var builder = WebApplication.CreateBuilder();
        
        // Add client services
        // Configure HTTP client with the new standardized library
        builder.Services.AddStarConflictsHttpClients(builder.Configuration, clientName: "GameApi", client =>
        {
            client.BaseAddress = new Uri("http://127.0.0.1:" + _port);
        });

        if (includeClientServices)
        {
            // Register core services
            builder.Services.AddSingleton<IClientWorldStore, ClientWorldStore>();
            builder.Services.AddSingleton<IGameRenderer, TestRenderer>();
            builder.Services.AddSingleton<IViewFactory, TestViewFactory>(); // Use TestViewFactory for testing
            builder.Services.AddSingleton<RenderContext>();
            builder.Services.AddSingleton<GameCommandService>();
            builder.Services.AddSingleton<GameState>();

            // Register all views as IView implementations
            builder.Services.AddSingleton<IView, TestMenuView>();
            builder.Services.AddSingleton<IView, TestGalaxyView>();
            builder.Services.AddSingleton<IView, TestTacticalBattleView>();
            builder.Services.AddSingleton<IView, TestFleetFinderView>();
            builder.Services.AddSingleton<IView, TestGameOptionsView>();
            builder.Services.AddSingleton<IView, TestPlanetaryFinderView>();

            // Bind configuration
            builder.Services.Configure<GameClientConfiguration>(
                builder.Configuration.GetSection("GameClientConfiguration"));

            // Register infrastructure services
            builder.Services.AddSingleton<SignalRService>();
            builder.Services.AddHostedService<ClientServiceHost>();

            // Register client initialization services
            builder.Services.AddSingleton<IClientIdentityService, ClientIdentityService>();
            builder.Services.AddSingleton<IClientInitializer, ClientInitializer>();
            
            // Add client services
            builder.Services.AddSingleton<IClientWorldStore, ClientWorldStore>();
        }
        
        // Use the same shared document store as RavenDbDataSourceAttribute
        _documentStore = SharedDocumentStore.CreateStore("test-database-" + Guid.NewGuid().ToString("N"));
        builder.Services.AddSingleton(_documentStore);
        builder.Services.AddScoped<IAsyncDocumentSession>(sp => 
            sp.GetRequiredService<IDocumentStore>().OpenAsyncSession());
        
        AddInMemoryConfigurationOverrides(builder);
        OpenSqliteConnectionAndConfigureDbContext(builder);

        // Set the log level of "Microsoft.EntityFrameworkCore.Database.Command" to "Warning" to reduce noise
        builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
        
        // Register test-specific services instead of production StartupHelper
        builder.Services.AddSingleton<IEventStore, RavenEventStore>();
        builder.Services.AddSingleton(typeof(CommandQueue<IGameEvent>));
        builder.Services.AddSingleton<SessionAggregateManager>();
        builder.Services.AddSingleton<WorldFactory>();
        
        builder.Services.AddScoped<SessionService>();
        builder.Services.AddScoped<WorldService>();
        builder.Services.AddScoped<LeaderboardService>();
        
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
        
        // Add JWT authentication
        builder.Services.AddAuthentication("Test")
            .AddJwtBearer("Test", options =>
            {
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = false,
                    RequireExpirationTime = false,
                    ValidateIssuerSigningKey = false
                };
            });
        
        // Add API versioning
        builder.Services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
        });
        
        // Add hosted services for full application testing
        // services.AddHostedService<GameUpdateService>();
        // services.AddHostedService<AiTurnService>();
        // services.AddHostedService<ProjectionService>();
        // services.AddHostedService<EventBroadcastService>();
        
        
        // Configure the web application
        builder.WebHost.UseUrls($"http://localhost:{_port}");
        
        _app = builder.Build();
        _app.Urls.Add($"http://localhost:{_port}");
        
        // Configure the HTTP request pipeline for tests
        ConfigureTestApplication(_app);

        // Create the API client
        _apiClient = new HttpApiClient(_app.Services.GetRequiredService<IHttpClientFactory>(), "GameApi");
    }

    private void OpenSqliteConnectionAndConfigureDbContext(WebApplicationBuilder builder)
    {
        _sqliteConnection.Open();

        builder.Services.AddDbContext<GameDbContext>(options =>
        {
            options.UseSqlite(_sqliteConnection)
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors();
        });
    }

    private void AddInMemoryConfigurationOverrides(WebApplicationBuilder builder)
    {
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string>
        {
            ["ConnectionStrings:gameDb"] = "DataSource=:memory:",
            ["ConnectionStrings:ravenDb"] = "http://localhost:8080",
            ["TokenProviderOptions:TokenEndpoint"] = "http://localhost:" + _port + "/token",
            ["TokenProviderOptions:ClientId"] = "test-client",
            ["TokenProviderOptions:Secret"] = Constants.Secret,
            ["GameClientConfiguration:GameServerHubUrl"] = "http://localhost:" + _port + "/gamehub",
            ["GameClientConfiguration:GameApiBaseUrl"] = "http://localhost:" + _port,
            ["GameClientConfiguration:DefaultSessionName"] = "Test Session",
            ["GameClientConfiguration:DefaultSessionType"] = "Multiplayer"
        }!);
    }

    private void ConfigureTestApplication(WebApplication app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
        MinimalApiHelper.MapMinimalApis(app);
        app.MapOpenApi();
        app.MapHub<WorldHub>("/gamehub");
        app.UseCors();
        
        // Ensure database is created with retry logic
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<GameDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<TestHostApplication>>();
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var connectionString = configuration.GetConnectionString("gameDb");
            if (!string.IsNullOrEmpty(connectionString))
            {
                var safeConnectionString = connectionString.Replace("Password=", "Password=***");
                logger.LogInformation("Using connection string: {ConnectionString}", safeConnectionString);
                if (connectionString == "SET_BY_ASPIRE_OR_ENVIRONMENT")
                {
                    logger.LogWarning("The gameDb connection string is not set by Aspire or environment. Database will not work.");
                }
            }
            else
            {
                logger.LogWarning("No connection string found for 'gameDb'");
            }
            var maxRetries = 2;
            var retryDelay = TimeSpan.FromSeconds(1);
            for (var attempt = 1; attempt <= maxRetries; attempt++)
                try
                {
                    logger.LogInformation("Attempting to ensure database is created (attempt {Attempt}/{MaxRetries})", attempt, maxRetries);
                    db.Database.EnsureCreated();
                    logger.LogInformation("Database created successfully");
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to create database on attempt {Attempt}/{MaxRetries}", attempt, maxRetries);
                    if (attempt == maxRetries)
                    {
                        logger.LogError(ex, "Failed to create database after {MaxRetries} attempts. Application will continue but database operations may fail.", maxRetries);
                        break;
                    }
                    Thread.Sleep(retryDelay);
                }

            logger.LogInformation("Database created successfully");
        }
    }

    private static int FindRandomUnusedPort()
    {
        var listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 0);
        listener.Start();
        int port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    // Public properties for test access
    public WebApplication Server => _app;
    public WebApplication App => _app;
    public IHttpApiClient Client => _apiClient;
    public IDocumentStore DocumentStore => _documentStore ?? throw new InvalidOperationException("DocumentStore not initialized");
    public async Task UseGameDbContextAsync(Func<GameDbContext, Task> action)
    {
        await using var scope = _app.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GameDbContext>();
        await action(dbContext);
    }
    
    public int Port => _port;
    public string GetGameServerHubUrl() => $"http://localhost:{_port}/gamehub";

    public void Dispose()
    {
        if (_app is IAsyncDisposable asyncApp)
            asyncApp.DisposeAsync().AsTask().GetAwaiter().GetResult();
        else
            (_app as IDisposable)?.Dispose();
        
        _sqliteConnection.Dispose();
        
        try
        {
            if (Directory.Exists(_uniqueDataDir))
                Directory.Delete(_uniqueDataDir, true);
        }
        catch { /* ignore cleanup errors */ }
    }

    public string GetPortAsString() => _port.ToString();

    public async Task StartServerAsync(CancellationToken cancellationToken)
    {
        if (_app == null)
            throw new InvalidOperationException("Application is not initialized. Call the constructor first.");

        // Start the application if not already started
        if (!_app.Lifetime.ApplicationStarted.IsCancellationRequested && !_app.Lifetime.ApplicationStopping.IsCancellationRequested)
        {
            return; // Already started
        }

        // Ensure the application is started
        await _app.StartAsync(cancellationToken);
    }
} 
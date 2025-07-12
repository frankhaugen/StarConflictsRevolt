using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using StarConflictsRevolt.Clients.Http.Http;
using StarConflictsRevolt.Clients.Raylib.Services;
using StarConflictsRevolt.Server.WebApi.Datastore;
using StarConflictsRevolt.Server.WebApi.Helpers;
using StarConflictsRevolt.Server.WebApi.Services;
using StarConflictsRevolt.Tests.TestingInfrastructure;

namespace StarConflictsRevolt.Tests.TestingInfrastructure;

public class TestApplicationHost : IDisposable
{
    private readonly WebApplication _app;
    private readonly SqliteConnection _sqliteConnection;
    private readonly int _port;
    private readonly string _uniqueDataDir;
    private readonly IHttpApiClient _apiClient;

    public TestApplicationHost()
    {
        _port = FindRandomUnusedPort();
        _uniqueDataDir = Path.Combine(Path.GetTempPath(), $"StarConflictsRevoltTest_{Guid.NewGuid()}");
        _sqliteConnection = new SqliteConnection("DataSource=:memory:");
        
        var builder = WebApplication.CreateBuilder();
        
        InitializeRavenDbServer(builder);
        AddInMemoryConfigurationOverrides(builder);
        OpenSqliteConnectionAndConfigureDbContext(builder);

        // Set the log level of "Microsoft.EntityFrameworkCore.Database.Command" to "Warning" to reduce noise
        builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
        
        // Register test-specific services instead of production StartupHelper
        builder.Services.AddTestApplicationServicesWithoutHostedServices();
        
        // Configure the web application
        builder.WebHost.UseUrls($"http://localhost:{_port}");
        
        _app = builder.Build();
        _app.Urls.Add($"http://localhost:{_port}");
        
        // Configure the HTTP request pipeline for tests
        ConfigureTestApplication(_app);
        
        // Start the application
        _app.StartAsync().GetAwaiter().GetResult();

        // Create the API client
        _apiClient = new HttpApiClient(_app.Services.GetRequiredService<IHttpClientFactory>(), "test-client");
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
            ["TokenProviderOptions:Secret"] = "test-secret",
        }!);
    }

    private void InitializeRavenDbServer(WebApplicationBuilder builder)
    {
        // Use the same shared document store as RavenDbDataSourceAttribute
        var documentStore = SharedDocumentStore.Instance;
        builder.Services.AddSingleton(documentStore);
        builder.Services.AddScoped<IAsyncDocumentSession>(sp => 
            sp.GetRequiredService<IDocumentStore>().OpenAsyncSession());
    }

    private void ConfigureTestApplication(WebApplication app)
    {
        // Ensure database is created with retry logic
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<GameDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<TestApplicationHost>>();
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

        app.UseAuthentication();
        app.UseAuthorization();
        MinimalApiHelper.MapMinimalApis(app);
        app.MapOpenApi();
        app.MapHub<WorldHub>("/gamehub");
        app.UseCors();
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
    public IHttpApiClient Client => _apiClient;
    public IDocumentStore DocumentStore => SharedDocumentStore.Instance;
    public GameDbContext GameDbContext => _app.Services.GetRequiredService<GameDbContext>();
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
} 
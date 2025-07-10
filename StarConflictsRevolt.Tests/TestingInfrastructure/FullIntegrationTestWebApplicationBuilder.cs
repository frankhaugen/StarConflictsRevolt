using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Raven.Client.Documents;
using Raven.Embedded;
using StarConflictsRevolt.Clients.Shared;
using StarConflictsRevolt.Server.Datastore;
using StarConflictsRevolt.Server.GameEngine;
using StarConflictsRevolt.Server.WebApi;

namespace StarConflictsRevolt.Tests.TestingInfrastructure;

public class FullIntegrationTestWebApplicationBuilder : IDisposable
{
    private readonly WebApplicationBuilder _appBuilder = WebApplication.CreateBuilder();
    private WebApplication? _app;
    
    private readonly SqliteConnection _sqliteConnection = new("DataSource=:memory:");
    private IDocumentStore? _documentStore;

    private readonly int _port = FindRandomUnusedPort();
    
    // Static flag to prevent multiple RavenDB server starts
    private static bool _ravenServerStarted = false;
    private static readonly object _ravenLock = new();
    
    private readonly string _uniqueDbName = $"StarConflictsRevoltTest_{Guid.NewGuid()}";
    private readonly string _uniqueDataDir = Path.Combine(Path.GetTempPath(), $"StarConflictsRevoltTest_{Guid.NewGuid()}");
    
    public IConfigurationManager ConfigurationManager => _appBuilder.Configuration;
    public ILoggingBuilder LoggingBuilder => _appBuilder.Logging;
    public IServiceCollection Services => _appBuilder.Services;
    
    public WebApplication WebApplication => _app ??= Build();

    public WebApplication Build()
    {
        // Initialize the in-memory RavenDB document store
        lock (_ravenLock)
        {
            if (!_ravenServerStarted)
            {
                try
                {
                    EmbeddedServer.Instance.StartServer(new ServerOptions() 
                    {
                        DataDirectory = _uniqueDataDir, // Use unique directory per test
                    });
                    _ravenServerStarted = true;
                }
                catch (InvalidOperationException ex) when (ex.Message.Contains("already started"))
                {
                    // Server is already started, which is fine
                    _ravenServerStarted = true;
                }
                catch (Exception e)
                {
                    // Ignore any other exceptions related to starting the server
                }
            }
        }
        
        _documentStore = EmbeddedServer.Instance.GetDocumentStore(_uniqueDbName); // Use unique DB name
        
        // Open the SQLite connection for the in-memory database
        _sqliteConnection.Open();
        
        // Create a new web application builder for the SignalR test server
        var builder = WebApplication.CreateBuilder();
        
        // Set log level of "Microsoft.EntityFrameworkCore.Database.Command" to "Warning" to reduce noise in the console output
        builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
        
        // Register SignalR services
        GameEngineStartupHelper.RegisterGameEngineServices(builder);
        
        builder.Services.AddSingleton<IClientWorldStore, ClientWorldStore>(); // Register the client world store
        
        builder.Services.AddDbContext<GameDbContext>(options =>
        {
            // Configure the database context with an in-memory database for testing
            options.UseSqlite(_sqliteConnection) // Use an in-memory SQLite database
                   .EnableSensitiveDataLogging() // Enable detailed logging for debugging
                   .EnableDetailedErrors(); // Enable detailed error messages
        });
        
        // Register the document store as a singleton service
        builder.Services.AddSingleton(_documentStore!);
        
        WebApiStartupHelper.RegisterServices(builder);
        
        // Configure the web application with the necessary services and middleware
        builder.WebHost.UseUrls($"http://localhost:{_port}"); // Set the URL for the server
        builder.Services.AddControllers(); // Add controllers or other services as needed
        
        // Configure the web application with the necessary services and middleware
        var app = builder.Build();
        
        // Set port for the application:
        app.Urls.Add($"http://localhost:{_port}");
        
        // Configure the HTTP request pipeline
        WebApiStartupHelper.Configure(app);
        // Configure the HTTP request pipeline for SignalR
        GameEngineStartupHelper.ConfigureGameEngine(app);
        
        return app;
    }
    
    public string GetGameServerHubUrl()
    {
        // Return the URL for the game server hub
        return $"http://localhost:{_port}/gamehub"; // Adjust the port as needed
    }
    
    private static int FindRandomUnusedPort()
    {
        // This method can be used to find a random unused port for the server
        var listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 0);
        listener.Start();
        int port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_app is IAsyncDisposable asyncApp)
            asyncApp.DisposeAsync().AsTask().GetAwaiter().GetResult();
        else
            ((IDisposable)_app)?.Dispose();
        _sqliteConnection.Dispose();
        _documentStore?.Dispose();
        // Clean up the unique RavenDB data directory
        try
        {
            if (Directory.Exists(_uniqueDataDir))
                Directory.Delete(_uniqueDataDir, true);
        }
        catch { /* ignore cleanup errors */ }
    }

    public int GetPort()
    {
        // Return the port number the server is running on
        return _port;
    }
}
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using StarConflictsRevolt.Clients.Raylib.Services;
using StarConflictsRevolt.Server.WebApi.Datastore;
using StarConflictsRevolt.Server.WebApi.Helpers;
using StarConflictsRevolt.Tests.TestingInfrastructure;

namespace StarConflictsRevolt.Tests.TestingInfrastructure;

public class GameServerTestHost : IDisposable
{
    private readonly WebApplication _app;
    private readonly SqliteConnection _sqliteConnection;
    private IDocumentStore? _documentStore;
    private readonly int _port;
    private readonly string _uniqueDataDir;

    public GameServerTestHost()
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
        
        // Register all core services using StartupHelper
        StartupHelper.RegisterAllServices(builder);
        
        builder.Services.AddSingleton<IClientWorldStore, ClientWorldStore>();
        
        // Configure the web application
        builder.WebHost.UseUrls($"http://localhost:{_port}");
        
        _app = builder.Build();
        _app.Urls.Add($"http://localhost:{_port}");
        
        // Configure the HTTP request pipeline
        StartupHelper.Configure(_app);
        
        // Start the application
        _app.StartAsync().GetAwaiter().GetResult();
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
        _documentStore = SharedDocumentStore.Instance;
        builder.Services.AddSingleton(_documentStore);
        builder.Services.AddScoped<IAsyncDocumentSession>(sp => 
            sp.GetRequiredService<IDocumentStore>().OpenAsyncSession());
    }

    public string GetGameServerHubUrl() => $"http://localhost:{_port}/gamehub";
    public int GetPort() => _port;
    public WebApplication App => _app;
    public IDocumentStore DocumentStore => _documentStore ?? throw new InvalidOperationException("DocumentStore not initialized");
    public IAsyncDocumentSession CreateSession() => DocumentStore.OpenAsyncSession();

    private static int FindRandomUnusedPort()
    {
        var listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 0);
        listener.Start();
        int port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    public void Dispose()
    {
        if (_app is IAsyncDisposable asyncApp)
            asyncApp.DisposeAsync().AsTask().GetAwaiter().GetResult();
        else
            (_app as IDisposable)?.Dispose();
        
        _sqliteConnection.Dispose();
        _documentStore?.Dispose();
        
        try
        {
            if (Directory.Exists(_uniqueDataDir))
                Directory.Delete(_uniqueDataDir, true);
        }
        catch { /* ignore cleanup errors */ }
    }
} 
using Microsoft.AspNetCore.Builder;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Raven.Client.Documents;
using Raven.Embedded;
using StarConflictsRevolt.Clients.Shared;
using StarConflictsRevolt.Server.Datastore;
using StarConflictsRevolt.Server.GameEngine;

namespace StarConflictsRevolt.Tests.TestingInfrastructure;

public class SignalRTestServer : IDisposable
{
    private readonly SqliteConnection _sqliteConnection = new("DataSource=:memory:");
    private IDocumentStore? _documentStore;

    private readonly int _port = FindRandomUnusedPort();
    
    public WebApplication GetWebApplication()
    {
        // Initialize the in-memory RavenDB document store
        try
        {
            EmbeddedServer.Instance.StartServer(new ServerOptions() 
            {
                // ServerDirectory = "StarConflictsRevoltTestServer", // Specify a directory for the server
                DataDirectory = "StarConflictsRevoltTest", // Specify a directory for the in-memory database
                // ServerUrl = "http://localhost:8181", // Set the server URL
            });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        _documentStore = EmbeddedServer.Instance.GetDocumentStore("StarConflictsRevolt");
        
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
        
        // Configure the web application with the necessary services and middleware
        var app = builder.Build();
        
        // Set port for the application:
        app.Urls.Add($"http://localhost:{_port}");
        
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
        _sqliteConnection.Dispose();
        _documentStore?.Dispose();
        try
        {
            EmbeddedServer.Instance.Dispose();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error shutting down embedded server: {e.Message}");
        }
    }
}
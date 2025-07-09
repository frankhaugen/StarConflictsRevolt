using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace StarConflictsRevolt.Tests.TestingInfrastructure;

/// <summary>
/// A full application server for testing purposes for use with TUnit and the StarConflictsRevolt game's Web API, or any other part either using the Web API or the service provider.
/// </summary>
public class WebApiTestServer : IDisposable
{
    private readonly int _port = FindRandomUnusedPort();
    private string _scheme = "http";
    
    /// <summary>
    /// Initializes a new instance of the <see cref="WebHost" /> class.
    /// </summary>
    public WebApiTestServer()
    {
        // Optionally, you can set the scheme to "https" if needed
        // SetScheme("https");
    }
    
    public WebApplication GetWebApplication()
    {
        // Create a new web application builder for the test server
        var builder = WebApplication.CreateBuilder();
        
        // Configure the web application with the necessary services and middleware
        builder.WebHost.UseUrls($"{_scheme}://localhost:{_port}"); // Set the URL for the server
        builder.Services.AddControllers(); // Add controllers or other services as needed
        
        // You can add more services here, such as:
        
        // Build the web application
        var app = builder.Build();
        
        // Configure the HTTP request pipeline
        
        return app;
    }
    
    
    public HttpClient GetHttpClient()
    {
        // Create a new HttpClient instance for testing
        var client = new HttpClient
        {
            BaseAddress = new Uri($"{_scheme}://localhost:{_port}") // Adjust the base address as needed
        };

        // Optionally, configure the client with default headers, timeouts, etc.
        client.DefaultRequestHeaders.Add("Accept", "application/json");

        return client;
    }
    
    public void SetScheme(string scheme)
    {
        // Set the scheme (http or https) for the server
        _scheme = scheme;
    }
    
    public int GetPort()
    {
        // Return the port number the server is running on
        return _port;
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
        // TODO release managed resources here
    }
}
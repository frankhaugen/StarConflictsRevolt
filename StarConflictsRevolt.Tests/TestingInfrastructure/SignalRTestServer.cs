using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using StarConflictsRevolt.Clients.Shared;
using StarConflictsRevolt.Server.GameEngine;

namespace StarConflictsRevolt.Tests.TestingInfrastructure;

public class SignalRTestServer
{
    public WebApplication GetWebApplication()
    {
        // Create a new web application builder for the SignalR test server
        var builder = WebApplication.CreateBuilder();
        
        // Register SignalR services
        GameEngineStartupHelper.RegisterGameEngineServices(builder);
        
        builder.Services.AddSingleton<IClientWorldStore, ClientWorldStore>(); // Register the client world store
        
        // Configure the web application with the necessary services and middleware
        var app = builder.Build();
        
        // Configure the HTTP request pipeline for SignalR
        GameEngineStartupHelper.ConfigureGameEngine(app);
        
        return app;
    }

    public string GetGameServerHubUrl()
    {
        // Return the URL for the game server hub
        return "http://localhost:5000/gamehub"; // Adjust the port as needed
    }
}
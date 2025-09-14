using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StarConflictsRevolt.Clients.Models;
using StarConflictsRevolt.Clients.Shared.Authentication;
using StarConflictsRevolt.Clients.Shared.Communication;
using StarConflictsRevolt.Clients.Shared.Configuration;
using StarConflictsRevolt.Clients.Shared.User;

namespace StarConflictsRevolt.Clients.Shared;

/// <summary>
/// Shared service collection extensions that can be used by both Raylib and Bliss clients.
/// This provides a common way to register shared services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds shared services that are common to both Raylib and Bliss clients
    /// </summary>
    /// <param name="builder">Host application builder</param>
    /// <returns>Host application builder for chaining</returns>
    public static HostApplicationBuilder AddSharedClientServices(this HostApplicationBuilder builder)
    {
        // Bind configuration
        builder.Services.Configure<GameClientConfiguration>(
            builder.Configuration.GetSection("GameClientConfiguration"));

        // Register shared communication services
        builder.Services.AddSingleton<ISignalRService, SignalRService>();

        // Register shared authentication services
        builder.Services.AddSingleton<IClientIdentityService, ClientIdentityService>();

        // Register shared configuration services
        builder.Services.AddSingleton<IClientInitializer, ClientInitializer>();
        
        // Register shared user info services
        builder.Services.AddUserService();
        
        // Add authentication HTTP clients
        builder.Services.AddStarConflictsHttpClients(
            builder.Configuration,
            clientName: "GameApi",
            configureClient: client =>
            {
                // Configure the HTTP client if needed
                var apiBaseUrl = builder.Configuration["GameClientConfiguration:ApiBaseUrl"];
                if (!string.IsNullOrEmpty(apiBaseUrl))
                {
                    client.BaseAddress = new Uri(apiBaseUrl);
                }
            });
        
        return builder;
    }
} 
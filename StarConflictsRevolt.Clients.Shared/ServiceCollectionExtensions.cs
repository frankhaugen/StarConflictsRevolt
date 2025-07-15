using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StarConflictsRevolt.Clients.Http.TODO.Shared.Authentication;
using StarConflictsRevolt.Clients.Http.TODO.Shared.Communication;
using StarConflictsRevolt.Clients.Http.TODO.Shared.Configuration;
using StarConflictsRevolt.Clients.Models;

namespace StarConflictsRevolt.Clients.Http.TODO.Shared;

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
    }
} 
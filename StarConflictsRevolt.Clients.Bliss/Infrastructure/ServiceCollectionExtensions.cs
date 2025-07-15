using StarConflictsRevolt.Clients.Bliss.Core;
using StarConflictsRevolt.Clients.Bliss.Input;
using StarConflictsRevolt.Clients.Bliss.Rendering;
using StarConflictsRevolt.Clients.Bliss.Rendering.Views;
using StarConflictsRevolt.Clients.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace StarConflictsRevolt.Clients.Bliss.Infrastructure;

/// <summary>
/// Service collection extensions for configuring the Bliss client services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds all Bliss client services to the service collection.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddBlissClientServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure logging
        services.AddLogging(logging =>
        {
            logging.AddConsole();
            logging.AddDebug();
            logging.SetMinimumLevel(LogLevel.Debug);
        });

        // Register mock Bliss window
        var windowConfig = configuration.GetSection("GameClientConfiguration").Get<GameClientConfiguration>();
        var window = new object(); // Mock window - will be replaced with actual Bliss window
        services.AddSingleton(window);

        // Register core services (following SOLID principles from design document)
        services.AddSingleton<IRenderer2D, BlissRenderer>();
        services.AddSingleton<IInput, BlissInput>();
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IGameLoop, GameLoop>();

        // Register game state
        services.AddSingleton<GameState>();

        // Register view factory and views
        services.AddSingleton<IViewFactory, ViewFactory>();
        services.AddSingleton<IView, MenuView>();
        services.AddSingleton<IView, GameOptionsView>();
        services.AddSingleton<IView, GalaxyView>();

        // Bind configuration
        services.Configure<GameClientConfiguration>(
            configuration.GetSection("GameClientConfiguration"));

        return services;
    }
} 
using Microsoft.Extensions.DependencyInjection;
using StarConflictsRevolt.Clients.Bliss.Core.UI;
using StarConflictsRevolt.Clients.Bliss.Core.UI.Interfaces;
using StarConflictsRevolt.Clients.Bliss.Views;
using StarConflictsRevolt.Clients.Shared.Player;

namespace StarConflictsRevolt.Clients.Bliss.Infrastructure.Configuration;

/// <summary>
/// Extension methods for configuring services in the dependency injection container.
/// Follows the Dependency Inversion Principle by depending on abstractions.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds UI services to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddUIServices(this IServiceCollection services)
    {
        // Register core UI interfaces and implementations
        services.AddSingleton<IInputHandler, InputHandler>();
        services.AddPlayerProfileProvider();
        services.AddSingleton<IScreenManager, ScreenManager>();
        
        // Register UI scaling and text rendering services
        services.AddSingleton<UIScalingService>();
        services.AddSingleton<TextRenderingService>();
        
        // Register UI screens
        services.AddTransient<LandingScreen>();
        services.AddTransient<SinglePlayerSetupScreen>();
        services.AddTransient<GalaxyScreen>();
        
        // Register screen factory
        services.AddSingleton<IScreenFactory, ScreenFactory>();
        
        return services;
    }
    
    /// <summary>
    /// Configures the screen manager with all available screens.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection ConfigureScreenManager(this IServiceCollection services)
    {
        services.AddSingleton<IScreenManagerInitializer, ScreenManagerInitializer>();
        
        return services;
    }
} 
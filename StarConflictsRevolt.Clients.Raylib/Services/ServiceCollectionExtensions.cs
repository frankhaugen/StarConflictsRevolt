using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StarConflictsRevolt.Clients.Models;
using StarConflictsRevolt.Clients.Http;
using StarConflictsRevolt.Clients.Raylib.Services;
using StarConflictsRevolt.Clients.Raylib.Renderers;

namespace StarConflictsRevolt.Clients.Raylib.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddClientServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add custom file logging provider
        services.AddLogging(logging =>
        {
            logging.AddProvider(new FileLoggerProvider());
            logging.SetMinimumLevel(LogLevel.Debug);
        });

        // Configure HTTP client with the new standardized library
        services.AddStarConflictsHttpClients(configuration, clientName: "GameApi", client =>
        {
            client.BaseAddress = new Uri("http://webapi");
        });

        // Register core services
        services.AddSingleton<IClientWorldStore, ClientWorldStore>();
        services.AddSingleton<IGameRenderer, RaylibRenderer>();
        services.AddSingleton<IViewFactory, ViewFactory>();
        services.AddSingleton<RenderContext>();
        services.AddSingleton<GameCommandService>();
        services.AddSingleton<GameState>();

        // Register all views as IView implementations
        services.AddSingleton<IView, MenuView>();
        services.AddSingleton<IView, GalaxyView>();
        services.AddSingleton<IView, TacticalBattleView>();
        services.AddSingleton<IView, FleetFinderView>();
        services.AddSingleton<IView, GameOptionsView>();
        services.AddSingleton<IView, PlanetaryFinderView>();

        // Bind configuration
        services.Configure<GameClientConfiguration>(
            configuration.GetSection("GameClientConfiguration"));

        // Register infrastructure services
        services.AddSingleton<SignalRService>();
        services.AddHostedService<ClientServiceHost>();

        // Register client initialization services
        services.AddSingleton<IClientIdentityService, ClientIdentityService>();
        services.AddSingleton<IClientInitializer, ClientInitializer>();

        return services;
    }
} 
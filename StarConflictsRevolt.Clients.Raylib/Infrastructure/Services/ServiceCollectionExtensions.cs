using StarConflictsRevolt.Clients.Http;
using StarConflictsRevolt.Clients.Models;
using StarConflictsRevolt.Clients.Raylib.Core;
using StarConflictsRevolt.Clients.Raylib.Game.Commands;
using StarConflictsRevolt.Clients.Raylib.Game.World;
using StarConflictsRevolt.Clients.Raylib.Infrastructure.Authentication;
using StarConflictsRevolt.Clients.Raylib.Infrastructure.Communication;
using StarConflictsRevolt.Clients.Raylib.Infrastructure.Configuration;
using StarConflictsRevolt.Clients.Raylib.Infrastructure.Logging;
using StarConflictsRevolt.Clients.Raylib.Rendering.Core;
using StarConflictsRevolt.Clients.Raylib.Rendering.Views;

namespace StarConflictsRevolt.Clients.Raylib.Infrastructure.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddClientServices(this HostApplicationBuilder builder)
    {
        return AddClientServices(builder.Services, builder.Configuration);
    }

    private static IServiceCollection AddClientServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add custom file logging provider
        services.AddLogging(logging =>
        {
            logging.AddProvider(new FileLoggerProvider());
            logging.SetMinimumLevel(LogLevel.Debug);
        });

        // Configure HTTP client with the new standardized library
        services.AddStarConflictsHttpClients(configuration, "GameApi", client =>
        {
            var apiUrl = configuration.GetValue<string>("GameClientConfiguration:ApiUrl");
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
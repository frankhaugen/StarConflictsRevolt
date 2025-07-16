using Microsoft.Extensions.DependencyInjection;

namespace StarConflictsRevolt.Clients.Shared.Player;

public static class PlayerServiceCollectionExtensions
{
    public static IServiceCollection AddPlayerProfileProvider(this IServiceCollection services)
    {
        services.AddSingleton<IPlayerProfileProvider, PlayerProfileProvider>();
        return services;
    }
} 
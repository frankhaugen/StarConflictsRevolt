using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StarConflictsRevolt.Clients.Shared.Authentication.Configuration;
using StarConflictsRevolt.Clients.Shared.Authentication.Http;

namespace StarConflictsRevolt.Clients.Shared.Authentication;

public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Adds all HTTP client and authentication services for Star Conflicts Revolt clients.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration</param>
    /// <param name="clientName">The name of the HTTP client to register</param>
    /// <param name="configureClient">Optional action to configure the HTTP client</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddStarConflictsHttpClients(
        this IServiceCollection services,
        IConfiguration configuration,
        string clientName = "GameApi",
        Action<HttpClient>? configureClient = null)
    {
        // Register configuration
        services.Configure<TokenProviderOptions>(configuration.GetSection(nameof(TokenProviderOptions)));

        // Register TokenProvider HTTP client
        services.AddHttpClient("TokenProvider");

        // Register TokenProvider options
        services.AddOptions<TokenProviderOptions>()
            .Bind(configuration.GetSection(nameof(TokenProviderOptions)))
            .ValidateOnStart();

        // Register Token provider
        services.AddSingleton<ITokenProvider, CachingTokenProvider>();

        // Register JWT token handler
        services.AddTransient<JwtTokenHandler>();

        // Register main HTTP client with JWT handler
        var builder = services.AddHttpClient(clientName)
            .AddHttpMessageHandler<JwtTokenHandler>();

        if (configureClient != null) builder.ConfigureHttpClient(configureClient);

        // Register HTTP API client
        services.AddTransient<IHttpApiClient>(sp => new HttpApiClient(sp.GetRequiredService<IHttpClientFactory>(), clientName));

        return services;
    }
}
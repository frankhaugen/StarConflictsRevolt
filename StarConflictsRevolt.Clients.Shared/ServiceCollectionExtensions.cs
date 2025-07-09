using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;

namespace StarConflictsRevolt.Clients.Shared;

public static class ServiceCollectionExtensions
{
    public static void AddHttpApiClientWithAuth(this IServiceCollection services, string clientName, IConfiguration configuration, Action<HttpClient>? configure = null)
    {
        // Register configuration for the client
        services.Configure<TokenProviderOptions>(configuration.GetSection(nameof(TokenProviderOptions)));
        
        // Register TokenProvider HTTP client with service discovery
        services.AddHttpClient("TokenProvider")
            .AddServiceDiscovery();
        
        // Register Token provider
        services.AddSingleton<ITokenProvider, CachingTokenProvider>();
        // Ensure JwtTokenHandler is registered
        services.AddTransient<JwtTokenHandler>();
        var builder = services.AddHttpClient(clientName)
            .AddHttpMessageHandler<JwtTokenHandler>()
            .AddServiceDiscovery()
            ;
        if (configure != null)
            builder.ConfigureHttpClient(configure);
        services.AddTransient(sp => new HttpApiClient(sp.GetRequiredService<IHttpClientFactory>(), clientName));
    }
}
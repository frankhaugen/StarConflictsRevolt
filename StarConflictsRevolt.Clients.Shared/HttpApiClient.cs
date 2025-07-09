using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using System;

namespace StarConflictsRevolt.Clients.Shared;

// Required NuGet packages:
// - Microsoft.Extensions.Http
// - Microsoft.Extensions.Http.Polly
// - Polly
// Ensure these are referenced in your project file.
public class HttpApiClient
{
    private readonly IHttpClientFactory _factory;
    private readonly string _clientName;

    public HttpApiClient(IHttpClientFactory factory, string clientName)
    {
        _factory = factory;
        _clientName = clientName;
    }

    public HttpClient Client => _factory.CreateClient(_clientName);

    public async Task<T?> GetAsync<T>(string uri, CancellationToken ct = default)
        => await Client.GetFromJsonAsync<T>(uri, ct);

    public async Task<HttpResponseMessage> PostAsync<T>(string uri, T body, CancellationToken ct = default)
        => await Client.PostAsJsonAsync(uri, body, ct);

    public async Task<HttpResponseMessage> DeleteAsync(string uri, CancellationToken ct = default)
        => await Client.DeleteAsync(uri, ct);

    public async Task<HttpResponseMessage> PutAsync<T>(string uri, T body, CancellationToken ct = default)
        => await Client.PutAsJsonAsync(uri, body, ct);

    public static void AddHttpApiClient(IServiceCollection services, string clientName, Action<HttpClient>? configure = null)
    {
        var builder = services.AddHttpClient(clientName);
        if (configure != null)
            builder.ConfigureHttpClient(configure);
        services.AddTransient(sp => new HttpApiClient(sp.GetRequiredService<IHttpClientFactory>(), clientName));
    }

    public static void AddHttpApiClientWithAuth(IServiceCollection services, string clientName, Action<HttpClient>? configure = null)
    {
        // Register Token provider
        services.AddSingleton<ITokenProvider, CachingTokenProvider>();
        // Ensure JwtTokenHandler is registered
        services.AddTransient<JwtTokenHandler>();
        var builder = services.AddHttpClient(clientName)
            .AddHttpMessageHandler<JwtTokenHandler>()
            .AddPolicyHandler(GetRetryPolicy());
        if (configure != null)
            builder.ConfigureHttpClient(configure);
        services.AddTransient(sp => new HttpApiClient(sp.GetRequiredService<IHttpClientFactory>(), clientName));
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy() =>
        HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            .WaitAndRetryAsync(2, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
} 
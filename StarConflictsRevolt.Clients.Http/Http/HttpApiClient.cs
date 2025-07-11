using System.Net.Http.Json;

namespace StarConflictsRevolt.Clients.Http.Http;

public class HttpApiClient : IHttpApiClient
{
    private readonly IHttpClientFactory _factory;
    private readonly string _clientName;

    public HttpApiClient(IHttpClientFactory factory, string clientName)
    {
        _factory = factory;
        _clientName = clientName;
    }

    public HttpClient Client => _factory.CreateClient(_clientName);

    private async Task EnsureHealthAsync(CancellationToken ct = default)
    {
        var healthResponse = await Client.GetAsync("/health/game", ct);
        if (!healthResponse.IsSuccessStatusCode)
        {
            var content = await healthResponse.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"API health check failed: GET /health/game returned {(int)healthResponse.StatusCode} {healthResponse.ReasonPhrase}.\nResponse body: '{content}'.\nThis likely indicates a misconfiguration, server startup failure, or network issue. Please check the API base URL, server logs, and connectivity.");
        }
    }

    public async Task<T?> GetAsync<T>(string uri, CancellationToken ct = default)
    {
        await EnsureHealthAsync(ct);
        return await Client.GetFromJsonAsync<T>(uri, ct);
    }

    public async Task<HttpResponseMessage> PostAsync<T>(string uri, T body, CancellationToken ct = default)
    {
        await EnsureHealthAsync(ct);
        return await Client.PostAsJsonAsync(uri, body, ct);
    }

    public async Task<HttpResponseMessage> PutAsync<T>(string uri, T body, CancellationToken ct = default)
    {
        await EnsureHealthAsync(ct);
        return await Client.PutAsJsonAsync(uri, body, ct);
    }

    public async Task<HttpResponseMessage> DeleteAsync(string uri, CancellationToken ct = default)
    {
        await EnsureHealthAsync(ct);
        return await Client.DeleteAsync(uri, ct);
    }
} 
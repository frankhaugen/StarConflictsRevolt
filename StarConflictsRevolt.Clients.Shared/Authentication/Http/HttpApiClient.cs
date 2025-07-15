using System.Net.Http.Json;
using System.Text.Json;

namespace StarConflictsRevolt.Clients.Http.Http;

public class HttpApiClient : IHttpApiClient
{
    private readonly string _clientName;
    private readonly IHttpClientFactory _factory;

    public HttpApiClient(IHttpClientFactory factory, string clientName)
    {
        _factory = factory;
        _clientName = clientName;
    }

    public HttpClient Client => _factory.CreateClient(_clientName);

    public async Task<HttpResponseMessage> RetrieveHealthCheckAsync(CancellationToken ct)
    {
        return await Client.GetAsync("/health/game", ct);
    }

    public async Task<T?> GetAsync<T>(string uri, CancellationToken ct = default)
    {
        await EnsureHealthAsync(ct);
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        return await Client.GetFromJsonAsync<T>(uri, jsonOptions, ct);
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

    public async Task<bool> IsHealthyAsync(CancellationToken ct = default)
    {
        try
        {
            await EnsureHealthAsync(ct);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private async Task EnsureHealthAsync(CancellationToken ct = default)
    {
        var healthResponse = await RetrieveHealthCheckAsync(ct);
        if (!healthResponse.IsSuccessStatusCode)
        {
            var content = await healthResponse.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"API health check failed: GET /health/game returned {(int)healthResponse.StatusCode} {healthResponse.ReasonPhrase}.\nResponse body: '{content}'.\nThis likely indicates a misconfiguration, server startup failure, or network issue. Please check the API base URL, server logs, and connectivity.");
        }
    }
}
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

    public async Task<T?> GetAsync<T>(string uri, CancellationToken ct = default)
        => await Client.GetFromJsonAsync<T>(uri, ct);

    public async Task<HttpResponseMessage> PostAsync<T>(string uri, T body, CancellationToken ct = default)
        => await Client.PostAsJsonAsync(uri, body, ct);

    public async Task<HttpResponseMessage> PutAsync<T>(string uri, T body, CancellationToken ct = default)
        => await Client.PutAsJsonAsync(uri, body, ct);

    public async Task<HttpResponseMessage> DeleteAsync(string uri, CancellationToken ct = default)
        => await Client.DeleteAsync(uri, ct);
} 
namespace StarConflictsRevolt.Clients.Shared.Http;

public interface IHttpApiClient
{
    Task<HttpResponseMessage> RetrieveHealthCheckAsync(CancellationToken ct);
    /// <summary>Raw GET without health pre-check; for diagnostics.</summary>
    Task<HttpResponseMessage> GetResponseAsync(string uri, CancellationToken ct = default);
    Task<T?> GetAsync<T>(string uri, CancellationToken ct = default);
    Task<HttpResponseMessage> PostAsync<T>(string uri, T body, CancellationToken ct = default);
    Task<HttpResponseMessage> PutAsync<T>(string uri, T body, CancellationToken ct = default);
    Task<HttpResponseMessage> DeleteAsync(string uri, CancellationToken ct = default);
    Task<bool> IsHealthyAsync(CancellationToken ct = default);
}
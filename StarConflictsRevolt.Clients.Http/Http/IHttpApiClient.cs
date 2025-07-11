namespace StarConflictsRevolt.Clients.Http.Http;

public interface IHttpApiClient
{
    Task<T?> GetAsync<T>(string uri, CancellationToken ct = default);
    Task<HttpResponseMessage> PostAsync<T>(string uri, T body, CancellationToken ct = default);
    Task<HttpResponseMessage> PutAsync<T>(string uri, T body, CancellationToken ct = default);
    Task<HttpResponseMessage> DeleteAsync(string uri, CancellationToken ct = default);
} 
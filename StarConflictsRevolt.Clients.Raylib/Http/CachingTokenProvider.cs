using System.Net.Http.Json;
using Microsoft.Extensions.Options;

namespace StarConflictsRevolt.Clients.Raylib.Http;

public class CachingTokenProvider : ITokenProvider
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<CachingTokenProvider> _logger;
    private readonly TokenProviderOptions _options;
    private string? _cachedToken;
    private DateTime _expiresAt;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public CachingTokenProvider(IHttpClientFactory httpClientFactory, ILogger<CachingTokenProvider> logger, IOptions<TokenProviderOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _options = options.Value;
        _logger.LogInformation("CachingTokenProvider initialized with endpoint: {Endpoint}, ClientId: {ClientId}, Secret: {Secret}", 
            _options.TokenEndpoint, _options.ClientId, string.IsNullOrEmpty(_options.Secret) ? "EMPTY" : "SET");
        
        if (string.IsNullOrEmpty(_options.ClientId))
        {
            _logger.LogError("ClientId is null or empty in TokenProviderOptions");
        }
        if (string.IsNullOrEmpty(_options.Secret))
        {
            _logger.LogError("Secret is null or empty in TokenProviderOptions");
        }
    }

    public async Task<string> GetTokenAsync(CancellationToken ct = default)
    {
        if (_cachedToken != null && DateTime.UtcNow < _expiresAt.AddMinutes(-5))
        {
            _logger.LogDebug("Returning cached token");
            return _cachedToken;
        }

        await _lock.WaitAsync(ct);
        try
        {
            // Double-check after acquiring lock
            if (_cachedToken != null && DateTime.UtcNow < _expiresAt.AddMinutes(-5))
            {
                _logger.LogDebug("Returning cached token after lock");
                return _cachedToken;
            }

            _logger.LogInformation("Requesting new token from endpoint: {Endpoint}", _options.TokenEndpoint);
            _logger.LogDebug("Token request payload: ClientId={ClientId}, Secret={Secret}", 
                _options.ClientId, string.IsNullOrEmpty(_options.Secret) ? "EMPTY" : "SET");
            
            // Use HttpClientFactory to get a client with service discovery and resilience
            var client = _httpClientFactory.CreateClient("TokenProvider");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("User-Agent", "StarConflictsRevolt.Clients.Shared");
            client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
            
            _logger.LogDebug("Sending token request: {Request}", System.Text.Json.JsonSerializer.Serialize(_options));
            
            var response = await client.PostAsJsonAsync(_options.TokenEndpoint, _options, ct);
            
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken: ct);
            
            if (result == null || string.IsNullOrWhiteSpace(result.access_token))
            {
                _logger.LogError("Token endpoint did not return a valid token");
                throw new InvalidOperationException("Token endpoint did not return a valid token");
            }

            _cachedToken = result.access_token;
            _expiresAt = DateTime.UtcNow.AddSeconds(result.expires_in > 0 ? result.expires_in : 3600);
            
            _logger.LogInformation("Successfully obtained new token, expires at: {ExpiresAt}", _expiresAt);
            return _cachedToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to obtain token from endpoint: {Endpoint}", _options.TokenEndpoint);
            throw;
        }
        finally
        {
            _lock.Release();
        }
    }

    private class TokenResponse
    {
        public string access_token { get; set; } = string.Empty;
        public int expires_in { get; set; }
    }
}
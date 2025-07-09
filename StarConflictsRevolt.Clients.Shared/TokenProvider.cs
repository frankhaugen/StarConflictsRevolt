using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace StarConflictsRevolt.Clients.Shared;

public interface ITokenProvider
{
    Task<string> GetTokenAsync(CancellationToken ct = default);
}

public class TokenProviderOptions
{
    public string TokenEndpoint { get; set; } = "http://localhost:5153/token";
    public string ClientId { get; set; } = string.Empty;
    public string Secret { get; set; } = "changeme";
}

public class CachingTokenProvider : ITokenProvider
{
    private readonly HttpClient _client;
    private readonly ILogger<CachingTokenProvider> _logger;
    private readonly TokenProviderOptions _options;
    private string? _cachedToken;
    private DateTime _expiresAt;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public CachingTokenProvider(HttpClient client, ILogger<CachingTokenProvider> logger, IOptions<TokenProviderOptions> options)
    {
        _client = client;
        _logger = logger;
        _options = options.Value;
        _logger.LogInformation("CachingTokenProvider initialized with endpoint: {Endpoint}, ClientId: {ClientId}", 
            _options.TokenEndpoint, _options.ClientId);
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
            var response = await _client.PostAsJsonAsync(_options.TokenEndpoint, 
                new { client_id = _options.ClientId, secret = _options.Secret }, ct);
            
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
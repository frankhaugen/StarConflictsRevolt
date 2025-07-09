using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace StarConflictsRevolt.Clients.Shared;

public interface ITokenProvider
{
    Task<string> GetTokenAsync(CancellationToken ct = default);
}

public class CachingTokenProvider : ITokenProvider
{
    private readonly HttpClient _client;
    private readonly ILogger<CachingTokenProvider> _logger;
    private readonly string _tokenEndpoint;
    private readonly string _clientId;
    private readonly string _secret;
    private string? _cachedToken;
    private DateTime _expiresAt;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public CachingTokenProvider(HttpClient client, ILogger<CachingTokenProvider> logger, string tokenEndpoint, string clientId, string secret)
    {
        _client = client;
        _logger = logger;
        _tokenEndpoint = tokenEndpoint;
        _clientId = clientId;
        _secret = secret;
    }

    public async Task<string> GetTokenAsync(CancellationToken ct = default)
    {
        if (_cachedToken != null && DateTime.UtcNow < _expiresAt.AddMinutes(-5))
            return _cachedToken;
        await _lock.WaitAsync(ct);
        try
        {
            if (_cachedToken != null && DateTime.UtcNow < _expiresAt.AddMinutes(-5))
                return _cachedToken;
            var response = await _client.PostAsJsonAsync(_tokenEndpoint, new { client_id = _clientId, secret = _secret }, ct);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken: ct);
            if (result == null || string.IsNullOrWhiteSpace(result.access_token))
                throw new InvalidOperationException("Token endpoint did not return a valid token");
            _cachedToken = result.access_token;
            _expiresAt = DateTime.UtcNow.AddHours(1); // TODO: parse expires_in if available
            return _cachedToken;
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
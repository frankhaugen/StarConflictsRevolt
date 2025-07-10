using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StarConflictsRevolt.Clients.Http.Configuration;
using System.Net.Http.Json;

namespace StarConflictsRevolt.Clients.Http.Authentication;

public class CachingTokenProvider : ITokenProvider
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptions<TokenProviderOptions> _options;
    private readonly ILogger<CachingTokenProvider> _logger;
    private string? _cachedToken;
    private DateTime _tokenExpiry = DateTime.MinValue;

    public CachingTokenProvider(
        IHttpClientFactory httpClientFactory,
        IOptions<TokenProviderOptions> options,
        ILogger<CachingTokenProvider> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options;
        _logger = logger;
    }

    public async Task<string> GetTokenAsync(CancellationToken ct = default)
    {
        // Check if we have a valid cached token
        if (!string.IsNullOrEmpty(_cachedToken) && DateTime.UtcNow < _tokenExpiry)
        {
            _logger.LogDebug("Using cached token");
            return _cachedToken;
        }

        _logger.LogInformation("Requesting new token from {TokenEndpoint}", _options.Value.TokenEndpoint);
        _logger.LogInformation("Using ClientId: {ClientId}, Secret: {Secret}", _options.Value.ClientId, _options.Value.Secret);

        try
        {
            var httpClient = _httpClientFactory.CreateClient("TokenProvider");
            
            var request = new
            {
                ClientId = _options.Value.ClientId,
                Secret = _options.Value.Secret
            };

            var response = await httpClient.PostAsJsonAsync(_options.Value.TokenEndpoint, request, ct);
            response.EnsureSuccessStatusCode();

            var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken: ct);
            if (tokenResponse?.access_token == null)
            {
                throw new InvalidOperationException("Token response did not contain access_token");
            }

            _cachedToken = tokenResponse.access_token;
            _tokenExpiry = DateTime.UtcNow.Add(_options.Value.TokenExpiry);

            _logger.LogInformation("Successfully obtained new token");
            return _cachedToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to obtain token from {TokenEndpoint}", _options.Value.TokenEndpoint);
            throw;
        }
    }

    public Task<string?> GetCachedTokenAsync(CancellationToken ct = default)
    {
        if (!string.IsNullOrEmpty(_cachedToken) && DateTime.UtcNow < _tokenExpiry)
        {
            return Task.FromResult<string?>(_cachedToken);
        }
        return Task.FromResult<string?>(null);
    }

    public Task InvalidateTokenAsync(CancellationToken ct = default)
    {
        _cachedToken = null;
        _tokenExpiry = DateTime.MinValue;
        _logger.LogDebug("Token cache invalidated");
        return Task.CompletedTask;
    }

    private record TokenResponse(string access_token, int expires_in, string token_type);
} 
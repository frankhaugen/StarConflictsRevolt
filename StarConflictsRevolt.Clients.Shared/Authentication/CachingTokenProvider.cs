using System.Collections.Concurrent;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StarConflictsRevolt.Clients.Models.Authentication;
using StarConflictsRevolt.Clients.Shared.Authentication.Configuration;

namespace StarConflictsRevolt.Clients.Shared.Authentication;

public class CachingTokenProvider(
    IHttpClientFactory httpClientFactory,
    IOptions<TokenProviderOptions> options,
    ILogger<CachingTokenProvider> logger) : ITokenProvider
{
    // Cache per client id so token identity matches session identity when using AuthClientIdContext
    private readonly ConcurrentDictionary<string, (string Token, DateTime Expiry)> _cache = new();

    public async Task<string> GetTokenAsync(CancellationToken ct = default)
    {
        var clientId = AuthClientIdContext.Current ?? options.Value.ClientId;
        if (string.IsNullOrWhiteSpace(clientId))
            clientId = options.Value.ClientId;

        if (_cache.TryGetValue(clientId, out var entry) && !string.IsNullOrEmpty(entry.Token) && DateTime.UtcNow < entry.Expiry)
        {
            logger.LogDebug("Using cached token for client {ClientId}", clientId.Length > 8 ? clientId[..8] + "…" : clientId);
            return entry.Token;
        }

        logger.LogInformation("Requesting new token from {TokenEndpoint} for client {ClientId}",
            options.Value.TokenEndpoint, clientId.Length > 8 ? clientId[..8] + "…" : clientId);

        try
        {
            var httpClient = httpClientFactory.CreateClient("TokenProvider");

            var request = new TokenRequest
            {
                ClientId = clientId,
                ClientSecret = options.Value.Secret
            };

            var response = await httpClient.PostAsJsonAsync(options.Value.TokenEndpoint, request, ct);
            response.EnsureSuccessStatusCode();

            var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>(ct);
            if (tokenResponse?.AccessToken == null) throw new InvalidOperationException("Token response did not contain access_token");

            _cache[clientId] = (tokenResponse.AccessToken, tokenResponse.ExpiresAt);

            logger.LogInformation("Successfully obtained new token for client {ClientId}", clientId.Length > 8 ? clientId[..8] + "…" : clientId);
            return tokenResponse.AccessToken;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "HTTP request failed when obtaining token from {TokenEndpoint}", options.Value.TokenEndpoint);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to obtain token from {TokenEndpoint}", options.Value.TokenEndpoint);
            throw;
        }
    }
}
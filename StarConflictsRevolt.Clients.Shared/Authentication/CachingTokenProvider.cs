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
    private string? _cachedToken;
    private DateTime _tokenExpiry = DateTime.MinValue;

    public async Task<string> GetTokenAsync(CancellationToken ct = default)
    {
        // Check if we have a valid cached token
        if (!string.IsNullOrEmpty(_cachedToken) && DateTime.UtcNow < _tokenExpiry)
        {
            logger.LogDebug("Using cached token");
            return _cachedToken;
        }

        logger.LogInformation("Requesting new token from {TokenEndpoint}", options.Value.TokenEndpoint);
        logger.LogInformation("Using ClientId: {ClientId}, Secret: {Secret}", options.Value.ClientId, options.Value.Secret);

        try
        {
            var httpClient = httpClientFactory.CreateClient("TokenProvider");

            var request = new TokenRequest
            {
                ClientId = options.Value.ClientId,
                ClientSecret = options.Value.Secret
            };

            var response = await httpClient.PostAsJsonAsync(options.Value.TokenEndpoint, request, ct);
            response.EnsureSuccessStatusCode();

            var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>(ct);
            if (tokenResponse?.AccessToken == null) throw new InvalidOperationException("Token response did not contain access_token");

            _cachedToken = tokenResponse.AccessToken;
            _tokenExpiry = tokenResponse.ExpiresAt;

            logger.LogInformation("Successfully obtained new token");
            return _cachedToken;
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
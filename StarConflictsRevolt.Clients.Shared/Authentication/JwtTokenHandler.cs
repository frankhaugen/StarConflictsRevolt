using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;

namespace StarConflictsRevolt.Clients.Shared.Authentication;

public class JwtTokenHandler : DelegatingHandler
{
    private readonly ILogger<JwtTokenHandler> _logger;
    private readonly ITokenProvider _tokenProvider;

    public JwtTokenHandler(ITokenProvider tokenProvider, ILogger<JwtTokenHandler> logger)
    {
        _tokenProvider = tokenProvider;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Attempting to get JWT token for request to {Uri}", request.RequestUri);
            var token = await _tokenProvider.GetTokenAsync(cancellationToken);
            
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Received empty token from token provider for request to {Uri}", request.RequestUri);
            }
            else
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                _logger.LogDebug("Successfully added Bearer token to request to {Uri}. Token length: {TokenLength}", 
                    request.RequestUri, token.Length);
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed when getting token for request to {Uri}. This will result in a 401 Unauthorized response.", request.RequestUri);
            // Continue without token - the server will handle authentication failure
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add Bearer token to request to {Uri}. This will result in a 401 Unauthorized response.", request.RequestUri);
            // Continue without token - the server will handle authentication failure
        }

        var response = await base.SendAsync(request, cancellationToken);
        
        // Log response status for authentication debugging
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            _logger.LogWarning("Received 401 Unauthorized response from {Uri}. Authentication may have failed.", request.RequestUri);
        }
        else if (response.IsSuccessStatusCode)
        {
            _logger.LogDebug("Successfully authenticated request to {Uri} with status {StatusCode}", 
                request.RequestUri, response.StatusCode);
        }
        
        return response;
    }
}
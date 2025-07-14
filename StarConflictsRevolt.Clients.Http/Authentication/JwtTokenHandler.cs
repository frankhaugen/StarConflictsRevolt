using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;

namespace StarConflictsRevolt.Clients.Http.Authentication;

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
            var token = await _tokenProvider.GetTokenAsync(cancellationToken);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            _logger.LogDebug("Added Bearer token to request to {Uri}", request.RequestUri);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add Bearer token to request to {Uri}. This will result in a 401 Unauthorized response.", request.RequestUri);
            // Continue without token - the server will handle authentication failure
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
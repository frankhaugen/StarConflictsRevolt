using System.Net.Http.Headers;

namespace StarConflictsRevolt.Clients.Shared;

public class JwtTokenHandler : DelegatingHandler
{
    private readonly ITokenProvider _tokenProvider;

    public JwtTokenHandler(ITokenProvider tokenProvider)
    {
        _tokenProvider = tokenProvider;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _tokenProvider.GetTokenAsync(cancellationToken);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await base.SendAsync(request, cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            // Force refresh and retry once
            token = await _tokenProvider.GetTokenAsync(cancellationToken); // Should force refresh in real impl
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            response = await base.SendAsync(request, cancellationToken);
        }
        return response;
    }
} 
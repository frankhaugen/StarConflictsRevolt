using Microsoft.JSInterop;

namespace StarConflictsRevolt.Clients.Blazor.Services;

/// <summary>
/// Uses sessionStorage for session id and localStorage for player name (see App.razor JS).
/// </summary>
public class ClientSessionStorage : IClientSessionStorage
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ILogger<ClientSessionStorage> _logger;

    public ClientSessionStorage(IJSRuntime jsRuntime, ILogger<ClientSessionStorage> logger)
    {
        _jsRuntime = jsRuntime;
        _logger = logger;
    }

    public async Task<Guid?> GetSessionIdAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var s = await _jsRuntime.InvokeAsync<string?>("getScrSessionId", cancellationToken);
            return Guid.TryParse(s, out var id) ? id : null;
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("statically rendered", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogDebug("Session storage not available during prerender");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not get session id from storage");
            return null;
        }
    }

    public async Task SetSessionIdAsync(Guid? sessionId, CancellationToken cancellationToken = default)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("setScrSessionId", sessionId?.ToString(), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not set session id in storage");
        }
    }

    public async Task<string?> GetPlayerNameAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var s = await _jsRuntime.InvokeAsync<string?>("getScrPlayerName", cancellationToken);
            return string.IsNullOrWhiteSpace(s) ? null : s.Trim();
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("statically rendered", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogDebug("Player name not available during prerender");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not get player name from storage");
            return null;
        }
    }

    public async Task SetPlayerNameAsync(string? playerName, CancellationToken cancellationToken = default)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("setScrPlayerName", string.IsNullOrWhiteSpace(playerName) ? null : playerName.Trim(), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not set player name in storage");
        }
    }
}

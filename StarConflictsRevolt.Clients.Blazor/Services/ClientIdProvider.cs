using Microsoft.JSInterop;

namespace StarConflictsRevolt.Clients.Blazor.Services;

/// <summary>
/// Gets a persistent player id from the browser via JS (localStorage).
/// Requires window.getPlayerId() to be defined in App.razor.
/// </summary>
public class ClientIdProvider : IClientIdProvider
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ILogger<ClientIdProvider> _logger;

    public ClientIdProvider(IJSRuntime jsRuntime, ILogger<ClientIdProvider> logger)
    {
        _jsRuntime = jsRuntime;
        _logger = logger;
    }

    public async Task<string> GetClientIdAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var id = await _jsRuntime.InvokeAsync<string>("getPlayerId", cancellationToken);
            return string.IsNullOrWhiteSpace(id) ? "anonymous" : id;
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("statically rendered", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogDebug("Client id not available during prerender, using fallback");
            return "anonymous-" + Guid.NewGuid().ToString("N")[..8];
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not get client id from browser, using fallback");
            return "anonymous-" + Guid.NewGuid().ToString("N")[..8];
        }
    }
}

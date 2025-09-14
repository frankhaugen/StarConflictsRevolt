using Microsoft.JSInterop;

namespace StarConflictsRevolt.Clients.Blazor.Services;

public interface IJavaScriptInteropService
{
    Task LogAsync(string message);
    Task LogErrorAsync(string message);
    Task LogWarningAsync(string message);
    Task LogInfoAsync(string message);
    Task LogDebugAsync(string message);
    Task LogButtonClickAsync(string buttonName);
    Task LogNavigationAsync(string url);
    Task LogServiceCallAsync(string service, string method, object? data = null);
    Task LogExceptionAsync(Exception exception);
}

public class JavaScriptInteropService : IJavaScriptInteropService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ILogger<JavaScriptInteropService> _logger;

    public JavaScriptInteropService(IJSRuntime jsRuntime, ILogger<JavaScriptInteropService> logger)
    {
        _jsRuntime = jsRuntime;
        _logger = logger;
    }

    public async Task LogAsync(string message)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("console.log", $"[Blazor] {message}");
            _logger.LogDebug("JS Log: {Message}", message);
        }
        catch (Exception ex) when (ex is InvalidOperationException && ex.Message.Contains("prerendering"))
        {
            // Ignore prerendering errors - this is expected during server-side rendering
            _logger.LogDebug("JS Log skipped during prerendering: {Message}", message);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to log to JavaScript console: {Message}", message);
        }
    }

    public async Task LogErrorAsync(string message)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("console.error", $"[Blazor ERROR] {message}");
            _logger.LogError("JS Error: {Message}", message);
        }
        catch (Exception ex) when (ex is InvalidOperationException && ex.Message.Contains("prerendering"))
        {
            _logger.LogDebug("JS Error skipped during prerendering: {Message}", message);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to log error to JavaScript console: {Message}", message);
        }
    }

    public async Task LogWarningAsync(string message)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("console.warn", $"[Blazor WARNING] {message}");
            _logger.LogWarning("JS Warning: {Message}", message);
        }
        catch (Exception ex) when (ex is InvalidOperationException && ex.Message.Contains("prerendering"))
        {
            _logger.LogDebug("JS Warning skipped during prerendering: {Message}", message);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to log warning to JavaScript console: {Message}", message);
        }
    }

    public async Task LogInfoAsync(string message)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("console.info", $"[Blazor INFO] {message}");
            _logger.LogInformation("JS Info: {Message}", message);
        }
        catch (Exception ex) when (ex is InvalidOperationException && ex.Message.Contains("prerendering"))
        {
            _logger.LogDebug("JS Info skipped during prerendering: {Message}", message);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to log info to JavaScript console: {Message}", message);
        }
    }

    public async Task LogDebugAsync(string message)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("console.debug", $"[Blazor DEBUG] {message}");
            _logger.LogDebug("JS Debug: {Message}", message);
        }
        catch (Exception ex) when (ex is InvalidOperationException && ex.Message.Contains("prerendering"))
        {
            _logger.LogDebug("JS Debug skipped during prerendering: {Message}", message);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to log debug to JavaScript console: {Message}", message);
        }
    }

    public async Task LogButtonClickAsync(string buttonName)
    {
        await LogInfoAsync($"Button clicked: {buttonName}");
        try
        {
            // Check if the function exists before calling it
            var functionExists = await _jsRuntime.InvokeAsync<bool>("typeof window.debugButtonClick === 'function'");
            if (functionExists)
            {
                await _jsRuntime.InvokeVoidAsync("debugButtonClick", buttonName);
            }
            else
            {
                _logger.LogDebug("debugButtonClick function not available yet, using console.log instead");
                await _jsRuntime.InvokeVoidAsync("console.log", $"Button clicked: {buttonName} (fallback)");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to call debugButtonClick: {ButtonName}", buttonName);
            // Fallback to basic console.log
            try
            {
                await _jsRuntime.InvokeVoidAsync("console.log", $"Button clicked: {buttonName} (fallback after error)");
            }
            catch (Exception fallbackEx)
            {
                _logger.LogError(fallbackEx, "Even fallback console.log failed for button: {ButtonName}", buttonName);
            }
        }
    }

    public async Task LogNavigationAsync(string url)
    {
        await LogInfoAsync($"Navigating to: {url}");
        try
        {
            await _jsRuntime.InvokeVoidAsync("debugNavigation", url);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to call debugNavigation: {Url}", url);
        }
    }

    public async Task LogServiceCallAsync(string service, string method, object? data = null)
    {
        var dataStr = data != null ? System.Text.Json.JsonSerializer.Serialize(data) : "null";
        await LogDebugAsync($"Service call: {service}.{method} with data: {dataStr}");
        try
        {
            await _jsRuntime.InvokeVoidAsync("debugServiceCall", service, method, data);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to call debugServiceCall: {Service}.{Method}", service, method);
        }
    }

    public async Task LogExceptionAsync(Exception exception)
    {
        var message = $"Exception: {exception.Message}\nStack: {exception.StackTrace}";
        await LogErrorAsync(message);
        _logger.LogError(exception, "Exception logged to JavaScript console");
    }
}

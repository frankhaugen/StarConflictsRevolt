using Microsoft.Playwright;
using TUnit.Playwright;

namespace StarConflictsRevolt.Tests.ClientTests.UITests;

/// <summary>
/// Configuration for Playwright tests
/// </summary>
public class PlaywrightTestConfiguration
{
    public async Task ConfigureAsync(IPlaywright playwright, IBrowser browser, IPage page)
    {
        // Configure browser context
        await page.SetViewportSizeAsync(1920, 1080);
        
        // Set up console logging
        page.Console += (_, msg) => Console.WriteLine($"Console: {msg.Type} - {msg.Text}");
        
        // Set up request/response logging
        page.Request += (_, request) => Console.WriteLine($"Request: {request.Method} {request.Url}");
        page.Response += (_, response) => Console.WriteLine($"Response: {response.Status} {response.Url}");
        
        // Handle uncaught exceptions
        page.PageError += (_, error) => Console.WriteLine($"Page Error: {error}");
    }
}

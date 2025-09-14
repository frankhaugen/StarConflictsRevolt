using Microsoft.Playwright;
using TUnit.Playwright;

namespace StarConflictsRevolt.Tests.ClientTests.UITests;

/// <summary>
/// Global test configuration for Playwright tests
/// </summary>
public class TestConfiguration : IPlaywrightTestConfiguration
{
    public async Task ConfigureAsync(IPlaywright playwright, IBrowser browser, IPage page)
    {
        // Configure browser settings
        await page.SetViewportSizeAsync(1920, 1080);
        
        // Set up console logging
        page.Console += (_, msg) => 
        {
            if (msg.Type == "error")
            {
                Console.WriteLine($"Console Error: {msg.Text}");
            }
        };
        
        // Set up request/response logging for debugging
        page.Request += (_, request) => 
        {
            if (request.Url.Contains("localhost"))
            {
                Console.WriteLine($"Request: {request.Method} {request.Url}");
            }
        };
        
        page.Response += (_, response) => 
        {
            if (response.Url.Contains("localhost"))
            {
                Console.WriteLine($"Response: {response.Status} {response.Url}");
            }
        };
        
        // Handle uncaught exceptions
        page.PageError += (_, error) => 
        {
            Console.WriteLine($"Page Error: {error}");
        };
        
        // Set up unhandled promise rejections
        page.Crash += (_, error) => 
        {
            Console.WriteLine($"Page Crash: {error}");
        };
    }
}

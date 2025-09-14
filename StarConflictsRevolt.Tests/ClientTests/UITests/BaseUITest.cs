using Microsoft.Playwright;
using TUnit.Playwright;
using TUnit;

namespace StarConflictsRevolt.Tests.ClientTests.UITests;

/// <summary>
/// Base class for UI tests using TUnit Playwright integration
/// </summary>
public abstract class BaseUITest : PageTest
{
    protected string BaseUrl => UITestRunner.GetBaseUrl();
    
    [Before(Class)]
    public static async Task Setup()
    {
        // Ensure test host is running before each test class
        await UITestRunner.SetupTestEnvironment();
    }
    
    [After(Class)]
    public static async Task Cleanup()
    {
        // Clean up after each test class
        await UITestRunner.CleanupTestEnvironment();
    }
    
    protected async Task NavigateToAppAsync()
    {
        // Navigate to the Blazor app
        await Page.GotoAsync(BaseUrl);
        
        // Wait for the app to load
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Wait for the main content to be visible
        await Page.WaitForSelectorAsync("h1", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible });
    }
    
    protected async Task WaitForConnectionStatusAsync(string expectedStatus)
    {
        var statusSelector = expectedStatus.Contains("Connected") 
            ? ".alert-success" 
            : ".alert-warning";
            
        await Page.WaitForSelectorAsync(statusSelector, new PageWaitForSelectorOptions 
        { 
            State = WaitForSelectorState.Visible,
            Timeout = 10000 
        });
    }
    
    protected async Task ClickButtonByTextAsync(string buttonText)
    {
        var button = Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = buttonText });
        await button.ClickAsync();
    }
    
    protected async Task ClickButtonByIconAsync(string iconClass)
    {
        var button = Page.Locator($"button .{iconClass}").First;
        await button.ClickAsync();
    }
    
    protected async Task WaitForNavigationAsync()
    {
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }
    
    protected async Task AssertElementVisibleAsync(string selector)
    {
        var element = Page.Locator(selector);
        await Assert.That(await element.IsVisibleAsync()).IsTrue();
    }
    
    protected async Task AssertElementContainsTextAsync(string selector, string expectedText)
    {
        var element = Page.Locator(selector).First;
        var text = await element.TextContentAsync();
        await Assert.That(text).Contains(expectedText);
    }
    
    protected async Task AssertElementContainsTextAsync(string selector)
    {
        var element = Page.Locator(selector);
        var text = await element.TextContentAsync();
        await Assert.That(text).IsNotNull();
        await Assert.That(text).IsNotEmpty();
    }
}

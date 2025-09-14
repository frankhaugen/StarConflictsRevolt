using Microsoft.Playwright;
using TUnit.Playwright;

namespace StarConflictsRevolt.Tests.ClientTests.UITests;

/// <summary>
/// Base class for UI tests using TUnit Playwright integration
/// </summary>
public abstract class BaseUITest : PageTest
{
    protected string BaseUrl => UITestRunner.GetBaseUrl();
    protected const string ServerUrl = "https://localhost:7002"; // API server URL
    
    protected override async Task OnTestSetupAsync()
    {
        await base.OnTestSetupAsync();
        
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
        await Assert.That(element).IsVisibleAsync();
    }
    
    protected async Task AssertElementContainsTextAsync(string selector, string expectedText)
    {
        var element = Page.Locator(selector);
        await Assert.That(element).ContainsTextAsync(expectedText);
    }
}

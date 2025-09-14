using Microsoft.Playwright;

namespace StarConflictsRevolt.Tests.ClientTests.UITests;

/// <summary>
/// UI tests for session management functionality
/// </summary>
public class SessionManagementTests : BaseUITest
{
    [Test]
    public async Task SessionsPage_DisplaysCorrectTitle()
    {
        // Navigate to sessions page
        await Page.GotoAsync($"{BaseUrl}/sessions");
        await WaitForNavigationAsync();
        
        // Verify the page title
        await AssertElementContainsTextAsync("h3", "Available Sessions");
    }
    
    [Test]
    public async Task SessionsPage_DisplaysRefreshButton()
    {
        // Navigate to sessions page
        await Page.GotoAsync($"{BaseUrl}/sessions");
        await WaitForNavigationAsync();
        
        // Verify refresh button is present
        await AssertElementVisibleAsync("button:has-text('Refresh')");
    }
    
    [Test]
    public async Task SessionsPage_DisplaysBackButton()
    {
        // Navigate to sessions page
        await Page.GotoAsync($"{BaseUrl}/sessions");
        await WaitForNavigationAsync();
        
        // Verify back button is present
        await AssertElementVisibleAsync("button:has-text('Back to Menu')");
    }
    
    [Test]
    public async Task SessionsPage_RefreshButton_LoadsSessions()
    {
        // Navigate to sessions page
        await Page.GotoAsync($"{BaseUrl}/sessions");
        await WaitForNavigationAsync();
        
        // Click refresh button
        await ClickButtonByTextAsync("Refresh");
        
        // Wait for loading to complete
        await Page.WaitForSelectorAsync(".spinner-border", new PageWaitForSelectorOptions 
        { 
            State = WaitForSelectorState.Detached,
            Timeout = 10000 
        });
        
        // Verify sessions are loaded (either sessions table or no sessions message)
        var hasSessions = await Page.Locator("table").IsVisibleAsync();
        var hasNoSessionsMessage = await Page.Locator(".text-center.text-muted").IsVisibleAsync();
        
        Assert.That(hasSessions || hasNoSessionsMessage, Is.True);
    }
    
    [Test]
    public async Task SessionsPage_BackButton_ReturnsToHome()
    {
        // Navigate to sessions page
        await Page.GotoAsync($"{BaseUrl}/sessions");
        await WaitForNavigationAsync();
        
        // Click back button
        await ClickButtonByTextAsync("Back to Menu");
        await WaitForNavigationAsync();
        
        // Verify we're back on the home page
        await Page.WaitForURLAsync("**/");
        await AssertElementContainsTextAsync("h1", "Star Conflicts Revolt");
    }
    
    [Test]
    public async Task SessionsPage_NoSessions_DisplaysCreateSessionButton()
    {
        // Navigate to sessions page
        await Page.GotoAsync($"{BaseUrl}/sessions");
        await WaitForNavigationAsync();
        
        // Wait for sessions to load
        await Page.WaitForSelectorAsync(".spinner-border", new PageWaitForSelectorOptions 
        { 
            State = WaitForSelectorState.Detached,
            Timeout = 10000 
        });
        
        // Check if no sessions message is displayed
        var noSessionsMessage = Page.Locator(".text-center.text-muted");
        if (await noSessionsMessage.IsVisibleAsync())
        {
            // Verify create session button is present
            await AssertElementVisibleAsync("button:has-text('Create Session')");
        }
    }
}

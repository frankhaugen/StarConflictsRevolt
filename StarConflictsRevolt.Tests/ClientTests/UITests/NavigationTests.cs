using Microsoft.Playwright;
using TUnit.Playwright;
using TUnit;

namespace StarConflictsRevolt.Tests.ClientTests.UITests;

/// <summary>
/// Comprehensive navigation tests for all main pages
/// </summary>
public class NavigationTests : BaseUITest
{
    [Test]
    [Timeout(30_000)]
    public async Task HomePage_AllMainMenuButtons_AreVisible(CancellationToken cancellationToken)
    {
        await NavigateToAppAsync();
        
        // Verify all main menu buttons are present and visible
        await AssertElementVisibleAsync("[data-testid='single-player-btn']");
        await AssertElementVisibleAsync("[data-testid='multiplayer-btn']");
        await AssertElementVisibleAsync("[data-testid='join-session-btn']");
        await AssertElementVisibleAsync("[data-testid='options-btn']");
    }
    
    [Test]
    [Timeout(30_000)]
    public async Task HomePage_SinglePlayerButton_NavigatesToSinglePlayer(CancellationToken cancellationToken)
    {
        await NavigateToAppAsync();
        
        // Click Single Player button
        await Page.GetByTestId("single-player-btn").ClickAsync();
        await WaitForNavigationAsync();
        
        // Verify navigation to single player page
        await Page.WaitForURLAsync("**/singleplayer");
        await AssertElementVisibleAsync("[data-testid='game-sidebar']");
        await AssertElementVisibleAsync("[data-testid='home-btn']");
    }
    
    [Test]
    [Timeout(30_000)]
    public async Task HomePage_MultiplayerButton_NavigatesToMultiplayer(CancellationToken cancellationToken)
    {
        await NavigateToAppAsync();
        
        // Click Multiplayer button
        await Page.GetByTestId("multiplayer-btn").ClickAsync();
        await WaitForNavigationAsync();
        
        // Verify navigation to multiplayer page
        await Page.WaitForURLAsync("**/multiplayer");
        await AssertElementVisibleAsync("h1");
    }
    
    [Test]
    [Timeout(30_000)]
    public async Task HomePage_JoinSessionButton_NavigatesToSessions(CancellationToken cancellationToken)
    {
        await NavigateToAppAsync();
        
        // Click Join Session button
        await Page.GetByTestId("join-session-btn").ClickAsync();
        await WaitForNavigationAsync();
        
        // Verify navigation to sessions page
        await Page.WaitForURLAsync("**/sessions");
        await AssertElementVisibleAsync("[data-testid='sessions-title']");
        await AssertElementVisibleAsync("[data-testid='refresh-btn']");
        await AssertElementVisibleAsync("[data-testid='back-btn']");
    }
    
    [Test]
    [Timeout(30_000)]
    public async Task HomePage_OptionsButton_NavigatesToOptions(CancellationToken cancellationToken)
    {
        await NavigateToAppAsync();
        
        // Click Options button
        await Page.GetByTestId("options-btn").ClickAsync();
        await WaitForNavigationAsync();
        
        // Verify navigation to options page
        await Page.WaitForURLAsync("**/options");
        await AssertElementVisibleAsync("h1");
    }
    
    [Test]
    [Timeout(30_000)]
    public async Task SinglePlayerPage_HomeButton_ReturnsToHome(CancellationToken cancellationToken)
    {
        // Navigate to single player page
        await Page.GotoAsync($"{BaseUrl}/singleplayer");
        await WaitForNavigationAsync();
        
        // Click home button
        await Page.GetByTestId("home-btn").ClickAsync();
        await WaitForNavigationAsync();
        
        // Verify return to home page
        await Page.WaitForURLAsync("**/");
        await AssertElementVisibleAsync("[data-testid='home-title']");
    }
    
    [Test]
    [Timeout(30_000)]
    public async Task SessionsPage_BackButton_ReturnsToHome(CancellationToken cancellationToken)
    {
        // Navigate to sessions page
        await Page.GotoAsync($"{BaseUrl}/sessions");
        await WaitForNavigationAsync();
        
        // Click back button
        await Page.GetByTestId("back-btn").ClickAsync();
        await WaitForNavigationAsync();
        
        // Verify return to home page
        await Page.WaitForURLAsync("**/");
        await AssertElementVisibleAsync("[data-testid='home-title']");
    }
    
    [Test]
    [Timeout(30_000)]
    public async Task TestPage_BackToHomeButton_ReturnsToHome(CancellationToken cancellationToken)
    {
        // Navigate to test page
        await Page.GotoAsync($"{BaseUrl}/test");
        await WaitForNavigationAsync();
        
        // Click back to home button
        await Page.GetByTestId("back-to-home").ClickAsync();
        await WaitForNavigationAsync();
        
        // Verify return to home page
        await Page.WaitForURLAsync("**/");
        await AssertElementVisibleAsync("[data-testid='home-title']");
    }
    
    [Test]
    [Timeout(30_000)]
    public async Task Navigation_AllPages_LoadCorrectly(CancellationToken cancellationToken)
    {
        var pages = new[]
        {
            ("/", "[data-testid='home-title']"),
            ("/singleplayer", "[data-testid='game-sidebar']"),
            ("/multiplayer", "h1"),
            ("/sessions", "[data-testid='sessions-title']"),
            ("/options", "h1"),
            ("/test", "[data-testid='test-page-title']"),
            ("/diagnostics", "h1"),
            ("/galaxy", "h1")
        };
        
        foreach (var (path, expectedElement) in pages)
        {
            await Page.GotoAsync($"{BaseUrl}{path}");
            await WaitForNavigationAsync();
            
            // Verify the page loaded with expected element
            await AssertElementVisibleAsync(expectedElement);
            
            // Verify URL is correct
            await Page.WaitForURLAsync($"**{path}");
        }
    }
    
    [Test]
    [Timeout(30_000)]
    public async Task Navigation_BreadcrumbNavigation_WorksCorrectly(CancellationToken cancellationToken)
    {
        // Start at home
        await NavigateToAppAsync();
        await AssertElementVisibleAsync("[data-testid='home-title']");
        
        // Navigate to sessions
        await Page.GetByTestId("join-session-btn").ClickAsync();
        await WaitForNavigationAsync();
        await Page.WaitForURLAsync("**/sessions");
        
        // Navigate back to home
        await Page.GetByTestId("back-btn").ClickAsync();
        await WaitForNavigationAsync();
        await Page.WaitForURLAsync("**/");
        
        // Navigate to single player
        await Page.GetByTestId("single-player-btn").ClickAsync();
        await WaitForNavigationAsync();
        await Page.WaitForURLAsync("**/singleplayer");
        
        // Navigate back to home
        await Page.GetByTestId("home-btn").ClickAsync();
        await WaitForNavigationAsync();
        await Page.WaitForURLAsync("**/");
        
        // Verify we're back at home
        await AssertElementVisibleAsync("[data-testid='home-title']");
    }
    
    [Test]
    [Timeout(30_000)]
    public async Task Navigation_DirectURLAccess_WorksCorrectly(CancellationToken cancellationToken)
    {
        var testCases = new[]
        {
            ("/test", "Test.razor page should load directly"),
            ("/sessions", "Sessions page should load directly"),
            ("/singleplayer", "SinglePlayer page should load directly"),
            ("/multiplayer", "Multiplayer page should load directly"),
            ("/options", "Options page should load directly")
        };
        
        foreach (var (url, description) in testCases)
        {
            await Page.GotoAsync($"{BaseUrl}{url}");
            await WaitForNavigationAsync();
            
            // Verify page loaded successfully
            await Assert.That(Page.Url).Contains(url);
            
            // Verify no error page is displayed
            var errorElement = Page.Locator(".alert-danger, .error-message");
            await Assert.That(await errorElement.IsVisibleAsync()).IsFalse();
        }
    }
    
    [Test]
    [Timeout(30_000)]
    public async Task Navigation_ButtonStates_UpdateCorrectly(CancellationToken cancellationToken)
    {
        await NavigateToAppAsync();
        
        // Verify buttons are initially enabled
        var singlePlayerBtn = Page.GetByTestId("single-player-btn");
        var multiplayerBtn = Page.GetByTestId("multiplayer-btn");
        var joinSessionBtn = Page.GetByTestId("join-session-btn");
        
        await Assert.That(await singlePlayerBtn.IsEnabledAsync()).IsTrue();
        await Assert.That(await multiplayerBtn.IsEnabledAsync()).IsTrue();
        await Assert.That(await joinSessionBtn.IsEnabledAsync()).IsTrue();
        
        // Click a button and verify loading state
        await singlePlayerBtn.ClickAsync();
        
        // Wait for navigation to complete
        await WaitForNavigationAsync();
        await Page.WaitForURLAsync("**/singleplayer");
        
        // Navigate back to home
        await Page.GetByTestId("home-btn").ClickAsync();
        await WaitForNavigationAsync();
        
        // Verify buttons are still enabled after navigation
        await Assert.That(await singlePlayerBtn.IsEnabledAsync()).IsTrue();
        await Assert.That(await multiplayerBtn.IsEnabledAsync()).IsTrue();
        await Assert.That(await joinSessionBtn.IsEnabledAsync()).IsTrue();
    }
    
    [Test]
    [Timeout(30_000)]
    public async Task Navigation_InvalidURL_ShowsErrorPage(CancellationToken cancellationToken)
    {
        // Navigate to non-existent page
        await Page.GotoAsync($"{BaseUrl}/nonexistent-page");
        await WaitForNavigationAsync();
        
        // Verify error page is displayed
        await AssertElementVisibleAsync("h1");
        
        // Verify error message or 404 content
        var pageContent = await Page.TextContentAsync("h1");
        await Assert.That(pageContent).IsNotNull();
    }
    
    [Test]
    [Timeout(30_000)]
    public async Task Navigation_BackButton_WorksInBrowser(CancellationToken cancellationToken)
    {
        // Navigate through multiple pages
        await NavigateToAppAsync();
        await Page.GetByTestId("join-session-btn").ClickAsync();
        await WaitForNavigationAsync();
        await Page.WaitForURLAsync("**/sessions");
        
        await Page.GetByTestId("back-btn").ClickAsync();
        await WaitForNavigationAsync();
        await Page.WaitForURLAsync("**/");
        
        // Use browser back button
        await Page.GoBackAsync();
        await WaitForNavigationAsync();
        
        // Should be back on sessions page
        await Page.WaitForURLAsync("**/sessions");
        await AssertElementVisibleAsync("[data-testid='sessions-title']");
        
        // Use browser forward button
        await Page.GoForwardAsync();
        await WaitForNavigationAsync();
        
        // Should be back on home page
        await Page.WaitForURLAsync("**/");
        await AssertElementVisibleAsync("[data-testid='home-title']");
    }
}

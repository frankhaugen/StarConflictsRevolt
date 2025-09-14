using Microsoft.Playwright;

namespace StarConflictsRevolt.Tests.ClientTests.UITests;

/// <summary>
/// UI tests for the home page functionality
/// </summary>
public class HomePageTests : BaseUITest
{
    [Test]
    [Timeout(30_000)]
    public async Task HomePage_DisplaysCorrectTitle(CancellationToken cancellationToken)
    {
        // Verify the main title is displayed
        await AssertElementContainsTextAsync("[data-testid='home-title']", "Star Conflicts Revolt");
        await AssertElementContainsTextAsync(".lead", "A modern reimagining of Star Wars: Rebellion");
    }
    
    [Test]
    [Timeout(30_000)]
    public async Task HomePage_DisplaysMainMenuButtons(CancellationToken cancellationToken)
    {
        // Verify all main menu buttons are present
        await AssertElementVisibleAsync("[data-testid='single-player-btn']");
        await AssertElementVisibleAsync("[data-testid='multiplayer-btn']");
        await AssertElementVisibleAsync("[data-testid='join-session-btn']");
        await AssertElementVisibleAsync("[data-testid='options-btn']");
    }
    
    [Test]
    [Timeout(30_000)]
    public async Task HomePage_SinglePlayerButton_NavigatesToSinglePlayer(CancellationToken cancellationToken)
    {
        // Click the Single Player button
        await Page.GetByTestId("single-player-btn").ClickAsync();
        
        // Wait for navigation
        await WaitForNavigationAsync();
        
        // Verify we're on the single player page
        await Page.WaitForURLAsync("**/singleplayer");
        await AssertElementVisibleAsync("h1");
    }
    
    [Test]
    [Timeout(30_000)]
    public async Task HomePage_MultiplayerButton_NavigatesToMultiplayer(CancellationToken cancellationToken)
    {
        // Click the Multiplayer button
        await Page.GetByTestId("multiplayer-btn").ClickAsync();
        
        // Wait for navigation
        await WaitForNavigationAsync();
        
        // Verify we're on the multiplayer page
        await Page.WaitForURLAsync("**/multiplayer");
        await AssertElementVisibleAsync("h1");
    }
    
    [Test]
    [Timeout(30_000)]
    public async Task HomePage_JoinSessionButton_NavigatesToSessions(CancellationToken cancellationToken)
    {
        // Click the Join Session button
        await Page.GetByTestId("join-session-btn").ClickAsync();
        
        // Wait for navigation
        await WaitForNavigationAsync();
        
        // Verify we're on the sessions page
        await Page.WaitForURLAsync("**/sessions");
        await AssertElementContainsTextAsync("[data-testid='sessions-title']", "Available Sessions");
    }
    
    [Test]
    [Timeout(30_000)]
    public async Task HomePage_DisplaysConnectionStatus(CancellationToken cancellationToken)
    {
        // Verify connection status is displayed
        var connectionAlert = Page.GetByTestId("connection-status");
        await Assert.That(await connectionAlert.IsVisibleAsync()).IsTrue();
        
        // The status should be either connected or not connected
        var alertText = await connectionAlert.TextContentAsync();
        await Assert.That(alertText).IsNotNull();
        var hasConnected = alertText!.Contains("Connected");
        var hasNotConnected = alertText.Contains("Not connected");
        await Assert.That(hasConnected || hasNotConnected).IsTrue();
    }
    
    [Test]
    [Timeout(30_000)]
    public async Task HomePage_BackButton_ReturnsToHome(CancellationToken cancellationToken)
    {
        // Navigate to sessions page first
        await Page.GetByTestId("join-session-btn").ClickAsync();
        await WaitForNavigationAsync();
        
        // Click back button
        await Page.GetByTestId("back-btn").ClickAsync();
        await WaitForNavigationAsync();
        
        // Verify we're back on the home page
        await Page.WaitForURLAsync("**/");
        await AssertElementContainsTextAsync("[data-testid='home-title']", "Star Conflicts Revolt");
    }
}

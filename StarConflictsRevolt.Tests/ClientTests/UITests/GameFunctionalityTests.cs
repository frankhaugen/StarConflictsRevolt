using Microsoft.Playwright;

namespace StarConflictsRevolt.Tests.ClientTests.UITests;

/// <summary>
/// UI tests for game functionality
/// </summary>
public class GameFunctionalityTests : BaseUITest
{
    [Test]
    [Timeout(30_000)]
    public async Task SinglePlayerGame_CreatesSession_DisplaysGameInterface(CancellationToken cancellationToken)
    {
        // Navigate to single player page
        await Page.GotoAsync($"{BaseUrl}/singleplayer");
        await WaitForNavigationAsync();
        
        // Wait for game to load
        await Page.WaitForSelectorAsync(".game-container", new PageWaitForSelectorOptions 
        { 
            State = WaitForSelectorState.Visible,
            Timeout = 10000 
        });
        
        // Verify game interface elements are present
        await AssertElementVisibleAsync(".col-md-3.bg-dark"); // Sidebar
        await AssertElementVisibleAsync(".game-container"); // Main game area
        await AssertElementContainsTextAsync("h5", "Game Controls");
    }
    
    [Test]
    [Timeout(30_000)]
    public async Task SinglePlayerGame_DisplaysGameControls(CancellationToken cancellationToken)
    {
        // Navigate to single player page
        await Page.GotoAsync($"{BaseUrl}/singleplayer");
        await WaitForNavigationAsync();
        
        // Wait for game to load
        await Page.WaitForSelectorAsync(".game-container");
        
        // Verify game control buttons are present
        await AssertElementVisibleAsync("button:has-text('Fleet Manager')");
        await AssertElementVisibleAsync("button:has-text('Planet Manager')");
        await AssertElementVisibleAsync("button:has-text('Diplomacy')");
        await AssertElementVisibleAsync("button:has-text('Research')");
    }
    
    [Test]
    [Timeout(30_000)]
    public async Task SinglePlayerGame_DisplaysResources(CancellationToken cancellationToken)
    {
        // Navigate to single player page
        await Page.GotoAsync($"{BaseUrl}/singleplayer");
        await WaitForNavigationAsync();
        
        // Wait for game to load
        await Page.WaitForSelectorAsync(".game-container");
        
        // Verify resource display
        await AssertElementContainsTextAsync("h6:has-text('Resources')", "Resources");
        await AssertElementContainsTextAsync("text=Credits:");
        await AssertElementContainsTextAsync("text=Materials:");
        await AssertElementContainsTextAsync("text=Fuel:");
    }
    
    [Test]
    [Timeout(30_000)]
    public async Task SinglePlayerGame_DisplaysGameStatus(CancellationToken cancellationToken)
    {
        // Navigate to single player page
        await Page.GotoAsync($"{BaseUrl}/singleplayer");
        await WaitForNavigationAsync();
        
        // Wait for game to load
        await Page.WaitForSelectorAsync(".game-container");
        
        // Verify game status display
        await AssertElementContainsTextAsync("h6:has-text('Game Status')", "Game Status");
        await AssertElementContainsTextAsync("text=Turn:");
        await AssertElementContainsTextAsync("text=Phase:");
    }
    
    [Test]
    [Timeout(30_000)]
    public async Task SinglePlayerGame_FleetManagerButton_OpensFleetManager(CancellationToken cancellationToken)
    {
        // Navigate to single player page
        await Page.GotoAsync($"{BaseUrl}/singleplayer");
        await WaitForNavigationAsync();
        
        // Wait for game to load
        await Page.WaitForSelectorAsync(".game-container");
        
        // Click fleet manager button
        await ClickButtonByTextAsync("Fleet Manager");
        
        // Verify fleet manager modal is displayed
        await AssertElementVisibleAsync(".fleet-manager");
        await AssertElementContainsTextAsync("h5", "Fleet Manager");
    }
    
    [Test]
    [Timeout(30_000)]
    public async Task SinglePlayerGame_PlanetManagerButton_OpensPlanetManager(CancellationToken cancellationToken)
    {
        // Navigate to single player page
        await Page.GotoAsync($"{BaseUrl}/singleplayer");
        await WaitForNavigationAsync();
        
        // Wait for game to load
        await Page.WaitForSelectorAsync(".game-container");
        
        // Click planet manager button
        await ClickButtonByTextAsync("Planet Manager");
        
        // Verify planet manager modal is displayed
        await AssertElementVisibleAsync(".planet-manager");
        await AssertElementContainsTextAsync("h5", "Planet Manager");
    }
    
    [Test]
    [Timeout(30_000)]
    public async Task SinglePlayerGame_DisplaysGameMessages(CancellationToken cancellationToken)
    {
        // Navigate to single player page
        await Page.GotoAsync($"{BaseUrl}/singleplayer");
        await WaitForNavigationAsync();
        
        // Wait for game to load
        await Page.WaitForSelectorAsync(".game-container");
        
        // Wait for welcome message to appear
        await Page.WaitForSelectorAsync(".game-messages", new PageWaitForSelectorOptions 
        { 
            State = WaitForSelectorState.Visible,
            Timeout = 5000 
        });
        
        // Verify game messages are displayed
        await AssertElementContainsTextAsync("Game Messages");
        await AssertElementContainsTextAsync("Welcome to Star Conflicts Revolt!");
    }
    
    [Test]
    [Timeout(30_000)]
    public async Task SinglePlayerGame_HomeButton_ReturnsToHome(CancellationToken cancellationToken)
    {
        // Navigate to single player page
        await Page.GotoAsync($"{BaseUrl}/singleplayer");
        await WaitForNavigationAsync();
        
        // Wait for game to load
        await Page.WaitForSelectorAsync(".game-container");
        
        // Click home button
        await ClickButtonByIconAsync("fa-home");
        await WaitForNavigationAsync();
        
        // Verify we're back on the home page
        await Page.WaitForURLAsync("**/");
        await AssertElementContainsTextAsync("h1", "Star Conflicts Revolt");
    }
}

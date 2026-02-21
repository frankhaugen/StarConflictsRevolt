using Microsoft.Playwright;
using TUnit.Playwright;
using TUnit;

namespace StarConflictsRevolt.Tests.ClientTests.UITests;

/// <summary>
/// Tests for button click functionality and console logging
/// </summary>
public class ButtonClickTests : BaseUITest
{
    [Test]
    [Timeout(30_000)]
    public async Task ButtonClicks_TriggerConsoleLogging(CancellationToken cancellationToken)
    {
        await Page.GotoAsync($"{BaseUrl}/test");
        await WaitForNavigationAsync();
        
        // Set up console message listener
        var consoleMessages = new List<string>();
        Page.Console += (_, msg) => 
        {
            consoleMessages.Add($"{msg.Type}: {msg.Text}");
        };
        
        // Click the test button
        await Page.GetByTestId("test-button-click").ClickAsync();
        
        // Wait for console messages
        await Task.Delay(2000);
        
        // Verify console messages were logged
        var hasDirectJS = consoleMessages.Any(msg => msg.Contains("DIRECT JS: Test button clicked from Blazor"));
        var hasButtonClick = consoleMessages.Any(msg => msg.Contains("Button clicked: test-button"));
        var hasBlazorInfo = consoleMessages.Any(msg => msg.Contains("[Blazor INFO]"));
        
        await Assert.That(hasDirectJS || hasButtonClick || hasBlazorInfo).IsTrue();
    }
    
    [Test]
    [Timeout(30_000)]
    public async Task ButtonClicks_UpdateUIState(CancellationToken cancellationToken)
    {
        await Page.GotoAsync($"{BaseUrl}/test");
        await WaitForNavigationAsync();
        
        // Initially should show no results
        await AssertElementVisibleAsync("[data-testid='no-results-message']");
        
        // Click multiple buttons
        await Page.GetByTestId("test-button-click").ClickAsync();
        await Page.GetByTestId("test-js-console-log").ClickAsync();
        await Page.GetByTestId("test-counter").ClickAsync();
        
        // Wait for results to appear
        await Page.WaitForSelectorAsync("[data-testid='test-results-list']", new PageWaitForSelectorOptions 
        { 
            State = WaitForSelectorState.Visible,
            Timeout = 5000 
        });
        
        // Verify results are displayed
        await AssertElementVisibleAsync("[data-testid='test-results-list']");
        
        // Verify no results message is gone
        var noResultsMessage = Page.Locator("[data-testid='no-results-message']");
        await Assert.That(await noResultsMessage.IsVisibleAsync()).IsFalse();
        
        // Verify results contain expected content
        var resultsList = Page.Locator("[data-testid='test-results-list']");
        var resultsText = await resultsList.TextContentAsync();
        
        await Assert.That(resultsText).Contains("Button clicked");
        await Assert.That(resultsText).Contains("JavaScript console log test completed");
        await Assert.That(resultsText).Contains("Counter incremented");
    }
    
    [Test]
    [Timeout(30_000)]
    public async Task ButtonClicks_HandleErrorsGracefully(CancellationToken cancellationToken)
    {
        await Page.GotoAsync($"{BaseUrl}/test");
        await WaitForNavigationAsync();
        
        // Set up console message listener for errors
        var errorMessages = new List<string>();
        Page.Console += (_, msg) => 
        {
            if (msg.Type == "error")
            {
                errorMessages.Add(msg.Text);
            }
        };
        
        // Click buttons that might trigger errors
        await Page.GetByTestId("test-button-click").ClickAsync();
        await Page.GetByTestId("test-js-console-log").ClickAsync();
        await Page.GetByTestId("test-pure-js").ClickAsync();
        
        // Wait for any potential errors
        await Task.Delay(2000);
        
        // Verify no JavaScript errors occurred
        await Assert.That(errorMessages.Count).IsEqualTo(0);
        
        // Verify UI still works after clicks
        await AssertElementVisibleAsync("[data-testid='test-results-list']");
    }
    
    [Test]
    [Timeout(30_000)]
    public async Task ButtonClicks_AreResponsive(CancellationToken cancellationToken)
    {
        await Page.GotoAsync($"{BaseUrl}/test");
        await WaitForNavigationAsync();
        
        var startTime = DateTime.UtcNow;
        
        // Click multiple buttons rapidly
        await Page.GetByTestId("test-button-click").ClickAsync();
        await Page.GetByTestId("test-counter").ClickAsync();
        await Page.GetByTestId("test-counter").ClickAsync();
        await Page.GetByTestId("test-counter").ClickAsync();
        
        var endTime = DateTime.UtcNow;
        var responseTime = endTime - startTime;
        
        // Verify response time is reasonable (less than 2 seconds)
        await Assert.That(responseTime.TotalSeconds).IsLessThan(2.0);
        
        // Verify all clicks were processed
        await Page.WaitForSelectorAsync("[data-testid='test-results-list']", new PageWaitForSelectorOptions 
        { 
            State = WaitForSelectorState.Visible,
            Timeout = 5000 
        });
        
        var resultsList = Page.Locator("[data-testid='test-results-list']");
        var resultsText = await resultsList.TextContentAsync();
        
        // Should have multiple counter increments
        var counterIncrements = System.Text.RegularExpressions.Regex.Matches(resultsText!, "Counter incremented").Count;
        await Assert.That(counterIncrements).IsGreaterThanOrEqualTo(3);
    }
    
    [Test]
    [Timeout(30_000)]
    public async Task ButtonClicks_WorkWithKeyboardNavigation(CancellationToken cancellationToken)
    {
        await Page.GotoAsync($"{BaseUrl}/test");
        await WaitForNavigationAsync();
        
        // Focus on the first button
        await Page.GetByTestId("test-button-click").FocusAsync();
        
        // Press Enter to activate the button
        await Page.Keyboard.PressAsync("Enter");
        
        // Wait for results
        await Page.WaitForSelectorAsync("[data-testid='test-results-list']", new PageWaitForSelectorOptions 
        { 
            State = WaitForSelectorState.Visible,
            Timeout = 5000 
        });
        
        // Verify the button click worked
        await AssertElementVisibleAsync("[data-testid='test-results-list']");
        
        // Tab to next button and press Enter
        await Page.Keyboard.PressAsync("Tab");
        await Page.Keyboard.PressAsync("Enter");
        
        // Wait a moment for the second click to process
        await Task.Delay(1000);
        
        // Verify both clicks were processed
        var resultsList = Page.Locator("[data-testid='test-results-list']");
        var resultsText = await resultsList.TextContentAsync();
        
        // Should have multiple results
        var resultCount = System.Text.RegularExpressions.Regex.Matches(resultsText!, "\\[\\d{2}:\\d{2}:\\d{2}\\]").Count;
        await Assert.That(resultCount).IsGreaterThanOrEqualTo(2);
    }
    
    [Test]
    [Timeout(30_000)]
    public async Task ButtonClicks_WorkWithDifferentBrowsers(CancellationToken cancellationToken)
    {
        // This test verifies that button clicks work consistently
        // The actual browser testing would be done by running tests with different browser configurations
        
        await Page.GotoAsync($"{BaseUrl}/test");
        await WaitForNavigationAsync();
        
        // Test basic button functionality
        await Page.GetByTestId("test-button-click").ClickAsync();
        
        // Verify click was registered
        await Page.WaitForSelectorAsync("[data-testid='test-results-list']", new PageWaitForSelectorOptions 
        { 
            State = WaitForSelectorState.Visible,
            Timeout = 5000 
        });
        
        // Test different button types
        await Page.GetByTestId("test-pure-js").ClickAsync();
        await Page.GetByTestId("test-counter").ClickAsync();
        
        // Verify all clicks worked
        var resultsList = Page.Locator("[data-testid='test-results-list']");
        var resultsText = await resultsList.TextContentAsync();
        
        await Assert.That(resultsText).Contains("Button clicked");
        await Assert.That(resultsText).Contains("Counter incremented");
    }
    
    [Test]
    [Timeout(30_000)]
    public async Task ButtonClicks_HandleConcurrentClicks(CancellationToken cancellationToken)
    {
        await Page.GotoAsync($"{BaseUrl}/test");
        await WaitForNavigationAsync();
        
        // Click multiple buttons simultaneously
        var tasks = new[]
        {
            Page.GetByTestId("test-button-click").ClickAsync(),
            Page.GetByTestId("test-js-console-log").ClickAsync(),
            Page.GetByTestId("test-counter").ClickAsync()
        };
        
        await Task.WhenAll(tasks);
        
        // Wait for all results to appear
        await Page.WaitForSelectorAsync("[data-testid='test-results-list']", new PageWaitForSelectorOptions 
        { 
            State = WaitForSelectorState.Visible,
            Timeout = 5000 
        });
        
        // Verify all clicks were processed
        var resultsList = Page.Locator("[data-testid='test-results-list']");
        var resultsText = await resultsList.TextContentAsync();
        
        await Assert.That(resultsText).Contains("Button clicked");
        await Assert.That(resultsText).Contains("JavaScript console log test completed");
        await Assert.That(resultsText).Contains("Counter incremented");
    }
    
    [Test]
    [Timeout(30_000)]
    public async Task ButtonClicks_VerifyTypeAttribute(CancellationToken cancellationToken)
    {
        await Page.GotoAsync($"{BaseUrl}/test");
        await WaitForNavigationAsync();
        
        // Verify all buttons have the correct type attribute
        var testButton = Page.GetByTestId("test-button-click");
        var jsLogButton = Page.GetByTestId("test-js-console-log");
        var counterButton = Page.GetByTestId("test-counter");
        var pureJsButton = Page.GetByTestId("test-pure-js");
        
        await Assert.That(await testButton.GetAttributeAsync("type")).IsEqualTo("button");
        await Assert.That(await jsLogButton.GetAttributeAsync("type")).IsEqualTo("button");
        await Assert.That(await counterButton.GetAttributeAsync("type")).IsEqualTo("button");
        await Assert.That(await pureJsButton.GetAttributeAsync("type")).IsEqualTo("button");
    }
    
    [Test]
    [Timeout(30_000)]
    public async Task ButtonClicks_VerifyAccessibility(CancellationToken cancellationToken)
    {
        await Page.GotoAsync($"{BaseUrl}/test");
        await WaitForNavigationAsync();
        
        // Verify buttons are accessible
        var testButton = Page.GetByTestId("test-button-click");
        var jsLogButton = Page.GetByTestId("test-js-console-log");
        var counterButton = Page.GetByTestId("test-counter");
        var pureJsButton = Page.GetByTestId("test-pure-js");
        
        // Check that buttons are focusable
        await testButton.FocusAsync();
        await Assert.That(await testButton.EvaluateAsync<bool>("el => el === document.activeElement")).IsTrue();
        
        await jsLogButton.FocusAsync();
        await Assert.That(await jsLogButton.EvaluateAsync<bool>("el => el === document.activeElement")).IsTrue();
        
        await counterButton.FocusAsync();
        await Assert.That(await counterButton.EvaluateAsync<bool>("el => el === document.activeElement")).IsTrue();
        
        await pureJsButton.FocusAsync();
        await Assert.That(await pureJsButton.EvaluateAsync<bool>("el => el === document.activeElement")).IsTrue();
        
        // Check that buttons have proper ARIA roles
        await Assert.That(await testButton.GetAttributeAsync("role")).IsEqualTo("button");
        await Assert.That(await jsLogButton.GetAttributeAsync("role")).IsEqualTo("button");
        await Assert.That(await counterButton.GetAttributeAsync("role")).IsEqualTo("button");
        await Assert.That(await pureJsButton.GetAttributeAsync("role")).IsEqualTo("button");
    }
}

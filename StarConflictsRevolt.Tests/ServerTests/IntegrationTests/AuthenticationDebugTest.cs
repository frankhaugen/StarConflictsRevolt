using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using StarConflictsRevolt.Clients.Models.Authentication;
using StarConflictsRevolt.Server.WebApi;
using StarConflictsRevolt.Tests.TestingInfrastructure;

namespace StarConflictsRevolt.Tests.ServerTests.IntegrationTests;

public class AuthenticationDebugTest
{
    [Test]
    [Timeout(60_000)]
    public async Task Debug_Authentication_Flow_Step_By_Step(CancellationToken cancellationToken)
    {
        await Context.Current.OutputWriter.WriteLineAsync("=== STARTING AUTHENTICATION DEBUG TEST ===");
        
        // Step 1: Create test host
        await Context.Current.OutputWriter.WriteLineAsync("Step 1: Creating TestHostApplication...");
        var testHost = new TestHostApplication(false);
        await Context.Current.OutputWriter.WriteLineAsync($"TestHost created with port: {testHost.Port}");
        
        // Step 2: Start server
        await Context.Current.OutputWriter.WriteLineAsync("Step 2: Starting server...");
        await testHost.StartServerAsync(cancellationToken);
        await Context.Current.OutputWriter.WriteLineAsync("Server started successfully");
        
        // Step 3: Test raw HTTP client to token endpoint
        await Context.Current.OutputWriter.WriteLineAsync("Step 3: Testing token endpoint with raw HttpClient...");
        var rawHttpClient = new HttpClient { BaseAddress = new Uri($"http://localhost:{testHost.Port}") };
        
        var tokenRequest = new { ClientId = "test-client", ClientSecret = Constants.Secret };
        await Context.Current.OutputWriter.WriteLineAsync($"Token request: ClientId={tokenRequest.ClientId}, Secret={tokenRequest.ClientSecret}");
        
        var tokenResponse = await rawHttpClient.PostAsJsonAsync("/token", tokenRequest, cancellationToken);
        await Context.Current.OutputWriter.WriteLineAsync($"Token response status: {tokenResponse.StatusCode}");
        
        if (!tokenResponse.IsSuccessStatusCode)
        {
            var errorContent = await tokenResponse.Content.ReadAsStringAsync(cancellationToken);
            await Context.Current.OutputWriter.WriteLineAsync($"Token endpoint failed: {errorContent}");
            await Assert.That(tokenResponse.IsSuccessStatusCode).IsTrue();
        }
        
        var tokenContent = await tokenResponse.Content.ReadAsStringAsync(cancellationToken);
        await Context.Current.OutputWriter.WriteLineAsync($"Token response content: {tokenContent}");
        
        // Parse the token response
        var tokenObj = await tokenResponse.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken);
        await Context.Current.OutputWriter.WriteLineAsync($"Parsed token: AccessToken={tokenObj?.AccessToken?.Substring(0, Math.Min(20, tokenObj.AccessToken?.Length ?? 0))}..., TokenType={tokenObj?.TokenType}");
        
        // Step 4: Test configured HTTP client
        await Context.Current.OutputWriter.WriteLineAsync("Step 4: Testing configured HTTP client...");
        var configuredHttpClient = testHost.GetHttpClient();
        await Context.Current.OutputWriter.WriteLineAsync("Configured HttpClient created");
        
        // Step 5: Test a protected endpoint with configured client
        await Context.Current.OutputWriter.WriteLineAsync("Step 5: Testing protected endpoint with configured client...");
        var sessionRequest = new { SessionName = "debug-test-session", SessionType = "Multiplayer" };
        await Context.Current.OutputWriter.WriteLineAsync($"Session request: {sessionRequest.SessionName}");
        
        var sessionResponse = await configuredHttpClient.PostAsJsonAsync("/game/session", sessionRequest, cancellationToken);
        await Context.Current.OutputWriter.WriteLineAsync($"Session response status: {sessionResponse.StatusCode}");
        
        if (!sessionResponse.IsSuccessStatusCode)
        {
            var errorContent = await sessionResponse.Content.ReadAsStringAsync(cancellationToken);
            await Context.Current.OutputWriter.WriteLineAsync($"Session creation failed: {errorContent}");
            
            // Let's check what headers were sent
            await Context.Current.OutputWriter.WriteLineAsync("Checking request headers...");
            if (configuredHttpClient.DefaultRequestHeaders.Authorization != null)
            {
                await Context.Current.OutputWriter.WriteLineAsync($"Authorization header present: {configuredHttpClient.DefaultRequestHeaders.Authorization}");
            }
            else
            {
                await Context.Current.OutputWriter.WriteLineAsync("No Authorization header found");
            }
        }
        else
        {
            var sessionContent = await sessionResponse.Content.ReadAsStringAsync(cancellationToken);
            await Context.Current.OutputWriter.WriteLineAsync($"Session created successfully: {sessionContent}");
        }
        
        // Step 6: Test with manual token
        await Context.Current.OutputWriter.WriteLineAsync("Step 6: Testing with manual token...");
        var manualHttpClient = new HttpClient { BaseAddress = new Uri($"http://localhost:{testHost.Port}") };
        manualHttpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenObj?.AccessToken);
        
        var manualSessionResponse = await manualHttpClient.PostAsJsonAsync("/game/session", sessionRequest, cancellationToken);
        await Context.Current.OutputWriter.WriteLineAsync($"Manual token session response status: {manualSessionResponse.StatusCode}");
        
        if (!manualSessionResponse.IsSuccessStatusCode)
        {
            var errorContent = await manualSessionResponse.Content.ReadAsStringAsync(cancellationToken);
            await Context.Current.OutputWriter.WriteLineAsync($"Manual token session creation failed: {errorContent}");
        }
        else
        {
            var sessionContent = await manualSessionResponse.Content.ReadAsStringAsync(cancellationToken);
            await Context.Current.OutputWriter.WriteLineAsync($"Manual token session created successfully: {sessionContent}");
        }
        
        // Step 7: Test health endpoint (should not require auth)
        await Context.Current.OutputWriter.WriteLineAsync("Step 7: Testing health endpoint...");
        var healthResponse = await rawHttpClient.GetAsync("/health", cancellationToken);
        await Context.Current.OutputWriter.WriteLineAsync($"Health response status: {healthResponse.StatusCode}");
        
        if (healthResponse.IsSuccessStatusCode)
        {
            var healthContent = await healthResponse.Content.ReadAsStringAsync(cancellationToken);
            await Context.Current.OutputWriter.WriteLineAsync($"Health content: {healthContent}");
        }
        
        // Step 8: Test root endpoint
        await Context.Current.OutputWriter.WriteLineAsync("Step 8: Testing root endpoint...");
        var rootResponse = await rawHttpClient.GetAsync("/", cancellationToken);
        await Context.Current.OutputWriter.WriteLineAsync($"Root response status: {rootResponse.StatusCode}");
        
        if (rootResponse.IsSuccessStatusCode)
        {
            var rootContent = await rootResponse.Content.ReadAsStringAsync(cancellationToken);
            await Context.Current.OutputWriter.WriteLineAsync($"Root content: {rootContent}");
        }
        
        await Context.Current.OutputWriter.WriteLineAsync("=== AUTHENTICATION DEBUG TEST COMPLETED ===");
        
        // Assertions
        await Assert.That(tokenResponse.IsSuccessStatusCode).IsTrue();
        await Assert.That(tokenObj).IsNotNull();
        await Assert.That(tokenObj!.AccessToken).IsNotEmpty();
    }
    
    [Test]
    [Timeout(60_000)]
    public async Task Debug_HttpClient_Configuration(CancellationToken cancellationToken)
    {
        await Context.Current.OutputWriter.WriteLineAsync("=== STARTING HTTP CLIENT CONFIGURATION DEBUG ===");
        
        var testHost = new TestHostApplication(false);
        await testHost.StartServerAsync(cancellationToken);
        
        await Context.Current.OutputWriter.WriteLineAsync("Testing different HTTP client configurations...");
        
        // Test 1: Raw HttpClient
        await Context.Current.OutputWriter.WriteLineAsync("Test 1: Raw HttpClient");
        var rawClient = new HttpClient { BaseAddress = new Uri($"http://localhost:{testHost.Port}") };
        var rawResponse = await rawClient.GetAsync("/health", cancellationToken);
        await Context.Current.OutputWriter.WriteLineAsync($"Raw client health response: {rawResponse.StatusCode}");
        
        // Test 2: Configured HttpClient
        await Context.Current.OutputWriter.WriteLineAsync("Test 2: Configured HttpClient");
        var configuredClient = testHost.GetHttpClient();
        var configuredResponse = await configuredClient.GetAsync("/health", cancellationToken);
        await Context.Current.OutputWriter.WriteLineAsync($"Configured client health response: {configuredResponse.StatusCode}");
        
        // Test 3: TestHost.Client (IHttpApiClient)
        await Context.Current.OutputWriter.WriteLineAsync("Test 3: TestHost.Client (IHttpApiClient)");
        var apiClient = testHost.Client;
        await Context.Current.OutputWriter.WriteLineAsync($"API client type: {apiClient.GetType().Name}");
        
        // Test 4: Check if JWT handler is configured
        await Context.Current.OutputWriter.WriteLineAsync("Test 4: Checking JWT handler configuration");
        var httpClientFactory = testHost.Server.Services.GetRequiredService<IHttpClientFactory>();
        var namedClient = httpClientFactory.CreateClient(Constants.HttpClientName);
        await Context.Current.OutputWriter.WriteLineAsync($"Named client type: {namedClient.GetType().Name}");
        
        // Test 5: Check token provider configuration
        await Context.Current.OutputWriter.WriteLineAsync("Test 5: Checking token provider configuration");
        var tokenProvider = testHost.Server.Services.GetService(typeof(StarConflictsRevolt.Clients.Http.Authentication.ITokenProvider)) as StarConflictsRevolt.Clients.Http.Authentication.ITokenProvider;
        await Context.Current.OutputWriter.WriteLineAsync($"Token provider type: {tokenProvider?.GetType().Name ?? "null"}");
        
        if (tokenProvider != null)
        {
            try
            {
                await Context.Current.OutputWriter.WriteLineAsync("Attempting to get token from provider...");
                var token = await tokenProvider.GetTokenAsync(cancellationToken);
                await Context.Current.OutputWriter.WriteLineAsync($"Token obtained: {token?.Substring(0, Math.Min(20, token?.Length ?? 0))}...");
            }
            catch (Exception ex)
            {
                await Context.Current.OutputWriter.WriteLineAsync($"Token provider failed: {ex.Message}");
                await Context.Current.OutputWriter.WriteLineAsync($"Exception type: {ex.GetType().Name}");
            }
        }
        
        await Context.Current.OutputWriter.WriteLineAsync("=== HTTP CLIENT CONFIGURATION DEBUG COMPLETED ===");
    }
} 
using System.Net.Http.Headers;
using System.Net.Http.Json;
using StarConflictsRevolt.Clients.Models.Authentication;
using StarConflictsRevolt.Server.WebApi.Infrastructure.Configuration;
using StarConflictsRevolt.Tests.TestingInfrastructure;

namespace StarConflictsRevolt.Tests.ServerTests.IntegrationTests;

public class SimpleAuthTest
{
    [Test]
    [Timeout(60_000)]
    public async Task Simple_Token_Test(CancellationToken cancellationToken)
    {
        Console.WriteLine("=== SIMPLE TOKEN TEST ===");

        // Create test host
        Console.WriteLine("Creating TestHostApplication...");
        var testHost = new TestHostApplication(false);
        Console.WriteLine($"TestHost created with port: {testHost.Port}");

        // Start server
        Console.WriteLine("Starting server...");
        await testHost.StartServerAsync(cancellationToken);
        Console.WriteLine("Server started successfully");

        // Test token endpoint with raw HttpClient
        Console.WriteLine("Testing token endpoint...");
        var httpClient = new HttpClient { BaseAddress = new Uri($"http://localhost:{testHost.Port}") };

        var tokenRequest = new { ClientId = "test-client", ClientSecret = Constants.Secret };
        Console.WriteLine($"Token request: ClientId={tokenRequest.ClientId}, Secret={tokenRequest.ClientSecret}");

        var tokenResponse = await httpClient.PostAsJsonAsync("/token", tokenRequest, cancellationToken);
        Console.WriteLine($"Token response status: {tokenResponse.StatusCode}");

        if (!tokenResponse.IsSuccessStatusCode)
        {
            var errorContent = await tokenResponse.Content.ReadAsStringAsync(cancellationToken);
            Console.WriteLine($"Token endpoint failed: {errorContent}");
            throw new Exception($"Token endpoint failed: {tokenResponse.StatusCode} - {errorContent}");
        }

        var tokenContent = await tokenResponse.Content.ReadAsStringAsync(cancellationToken);
        Console.WriteLine($"Token response content: {tokenContent}");

        // Parse the token response
        var tokenObj = await tokenResponse.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken);
        Console.WriteLine($"Parsed token: AccessToken={tokenObj?.AccessToken?.Substring(0, Math.Min(20, tokenObj.AccessToken?.Length ?? 0))}..., TokenType={tokenObj?.TokenType}");

        // Test session creation with the token
        Console.WriteLine("Testing session creation with token...");
        var sessionRequest = new { SessionName = "simple-test-session", SessionType = "Multiplayer" };

        // Add the token to the request
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenObj?.AccessToken);

        var sessionResponse = await httpClient.PostAsJsonAsync("/game/session", sessionRequest, cancellationToken);
        Console.WriteLine($"Session response status: {sessionResponse.StatusCode}");

        if (!sessionResponse.IsSuccessStatusCode)
        {
            var errorContent = await sessionResponse.Content.ReadAsStringAsync(cancellationToken);
            Console.WriteLine($"Session creation failed: {errorContent}");
            throw new Exception($"Session creation failed: {sessionResponse.StatusCode} - {errorContent}");
        }

        var sessionContent = await sessionResponse.Content.ReadAsStringAsync(cancellationToken);
        Console.WriteLine($"Session created successfully: {sessionContent}");

        Console.WriteLine("=== SIMPLE TOKEN TEST COMPLETED SUCCESSFULLY ===");
    }
}
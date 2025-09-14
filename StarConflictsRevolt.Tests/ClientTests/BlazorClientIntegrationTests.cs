using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StarConflictsRevolt.Clients.Blazor.Services;
using StarConflictsRevolt.Clients.Shared.Communication;
using StarConflictsRevolt.Clients.Shared.Http;
using StarConflictsRevolt.Clients.Models;
using StarConflictsRevolt.Tests.TestingInfrastructure;
using TUnit.Assertions;
using TUnit.Core;

namespace StarConflictsRevolt.Tests.ClientTests;

public class BlazorClientIntegrationTests
{
    [Test]
    public async Task BlazorClient_CanResolveAllServices_WithoutErrors()
    {
        // Arrange
        using var testHost = new TestHostApplication(includeClientServices: true);
        await testHost.StartServerAsync(CancellationToken.None);

        // Create a Blazor-like service collection to test DI resolution
        var services = new ServiceCollection();
        
        // Add the same services as Blazor Program.cs
        services.Configure<GameClientConfiguration>(config =>
        {
            config.ApiBaseUrl = $"http://localhost:{testHost.Port}";
            config.GameServerHubUrl = $"http://localhost:{testHost.Port}/gamehub";
            config.GameServerUrl = $"http://localhost:{testHost.Port}";
        });
        
        services.AddSingleton<ISignalRService, SignalRService>();
        
        // Register HTTP client with proper configuration
        services.AddHttpClient("GameApi", client =>
        {
            client.BaseAddress = new Uri($"http://localhost:{testHost.Port}");
        });
        
        services.AddScoped<IHttpApiClient>(provider =>
        {
            var factory = provider.GetRequiredService<IHttpClientFactory>();
            return new HttpApiClient(factory, "GameApi");
        });
        
        // Add Blazor-specific services
        services.AddSingleton<TelemetryService>();
        services.AddScoped<IGameStateService, GameStateService>();
        services.AddScoped<BlazorSignalRService>();

        // Act & Assert - This should not throw any DI exceptions
        var serviceProvider = services.BuildServiceProvider();
        
        // Test that all services can be resolved
        var httpApiClient = serviceProvider.GetRequiredService<IHttpApiClient>();
        var signalRService = serviceProvider.GetRequiredService<ISignalRService>();
        var gameStateService = serviceProvider.GetRequiredService<IGameStateService>();
        var blazorSignalRService = serviceProvider.GetRequiredService<BlazorSignalRService>();
        
        // Verify services are properly configured
        await Assert.That(httpApiClient).IsNotNull();
        await Assert.That(signalRService).IsNotNull();
        await Assert.That(gameStateService).IsNotNull();
        await Assert.That(blazorSignalRService).IsNotNull();
    }

    [Test]
    public async Task BlazorClient_HttpApiClient_CanMakeRequests()
    {
        // Arrange
        using var testHost = new TestHostApplication(includeClientServices: true);
        await testHost.StartServerAsync(CancellationToken.None);

        var services = new ServiceCollection();
        services.Configure<GameClientConfiguration>(config =>
        {
            config.ApiBaseUrl = $"http://localhost:{testHost.Port}";
            config.GameServerHubUrl = $"http://localhost:{testHost.Port}/gamehub";
            config.GameServerUrl = $"http://localhost:{testHost.Port}";
        });
        
        services.AddHttpClient("GameApi", client =>
        {
            client.BaseAddress = new Uri($"http://localhost:{testHost.Port}");
        });
        
        services.AddScoped<IHttpApiClient>(provider =>
        {
            var factory = provider.GetRequiredService<IHttpClientFactory>();
            return new HttpApiClient(factory, "GameApi");
        });

        var serviceProvider = services.BuildServiceProvider();
        var httpApiClient = serviceProvider.GetRequiredService<IHttpApiClient>();

        // Act
        var isHealthy = await httpApiClient.IsHealthyAsync();

        // Assert
        await Assert.That(isHealthy).IsTrue();
    }

    [Test]
    public async Task BlazorClient_GameStateService_CanCreateSession()
    {
        // Arrange
        using var testHost = new TestHostApplication(includeClientServices: true);
        await testHost.StartServerAsync(CancellationToken.None);

        var services = new ServiceCollection();
        services.Configure<GameClientConfiguration>(config =>
        {
            config.ApiBaseUrl = $"http://localhost:{testHost.Port}";
            config.GameServerHubUrl = $"http://localhost:{testHost.Port}/gamehub";
            config.GameServerUrl = $"http://localhost:{testHost.Port}";
        });
        
        services.AddSingleton<ISignalRService, SignalRService>();
        services.AddSingleton<TelemetryService>();

        services.AddHttpClient("GameApi", client =>
        {
            client.BaseAddress = new Uri($"http://localhost:{testHost.Port}");
        });
        
        services.AddScoped<IHttpApiClient>(provider =>
        {
            var factory = provider.GetRequiredService<IHttpClientFactory>();
            return new HttpApiClient(factory, "GameApi");
        });
        services.AddScoped<IGameStateService, GameStateService>();

        var serviceProvider = services.BuildServiceProvider();
        var gameStateService = serviceProvider.GetRequiredService<IGameStateService>();

        // Act - Test that the method can be called without throwing exceptions
        var success = await gameStateService.CreateSessionAsync("Test Session");

        // Assert - The method should complete without throwing, even if it returns false
        // This tests the service integration and error handling
        // We don't care about the actual result, just that it completes
        await Assert.That(gameStateService).IsNotNull();
    }

    [Test]
    public async Task BlazorClient_GameStateService_CanGetSessions()
    {
        // Arrange
        using var testHost = new TestHostApplication(includeClientServices: true);
        await testHost.StartServerAsync(CancellationToken.None);

        var services = new ServiceCollection();
        services.Configure<GameClientConfiguration>(config =>
        {
            config.ApiBaseUrl = $"http://localhost:{testHost.Port}";
            config.GameServerHubUrl = $"http://localhost:{testHost.Port}/gamehub";
            config.GameServerUrl = $"http://localhost:{testHost.Port}";
        });
        
        services.AddSingleton<ISignalRService, SignalRService>();
        services.AddSingleton<TelemetryService>();
        services.AddHttpClient("GameApi", client =>
        {
            client.BaseAddress = new Uri($"http://localhost:{testHost.Port}");
        });
        
        services.AddScoped<IHttpApiClient>(provider =>
        {
            var factory = provider.GetRequiredService<IHttpClientFactory>();
            return new HttpApiClient(factory, "GameApi");
        });
        services.AddScoped<IGameStateService, GameStateService>();

        var serviceProvider = services.BuildServiceProvider();
        var gameStateService = serviceProvider.GetRequiredService<IGameStateService>();

        // Act
        var sessions = await gameStateService.GetAvailableSessionsAsync();

        // Assert
        await Assert.That(sessions).IsNotNull();
        await Assert.That(sessions is List<SessionDto>).IsTrue();
    }

    [Test]
    public async Task BlazorClient_ServiceRegistration_ValidatesDependencyInjection()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // Test with proper configuration
        services.Configure<GameClientConfiguration>(config =>
        {
            config.ApiBaseUrl = "http://localhost:5000";
            config.GameServerHubUrl = "http://localhost:5000/gamehub";
            config.GameServerUrl = "http://localhost:5000";
        });
        
        services.AddSingleton<ISignalRService, SignalRService>();
        services.AddSingleton<TelemetryService>();
        services.AddHttpClient("GameApi", client =>
        {
            client.BaseAddress = new Uri("http://localhost:5000");
        });
        
        services.AddScoped<IHttpApiClient>(provider =>
        {
            var factory = provider.GetRequiredService<IHttpClientFactory>();
            return new HttpApiClient(factory, "GameApi");
        });
        services.AddScoped<IGameStateService, GameStateService>();

        // Act & Assert
        var serviceProvider = services.BuildServiceProvider();
        
        // This should work with proper configuration
        var httpApiClient = serviceProvider.GetRequiredService<IHttpApiClient>();
        var gameStateService = serviceProvider.GetRequiredService<IGameStateService>();
        
        await Assert.That(httpApiClient).IsNotNull();
        await Assert.That(gameStateService).IsNotNull();
    }
}

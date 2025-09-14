using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using StarConflictsRevolt.Clients.Blazor.Services;
using StarConflictsRevolt.Clients.Shared.Communication;
using StarConflictsRevolt.Clients.Shared.Http;
using StarConflictsRevolt.Clients.Models;
using TUnit.Assertions;
using TUnit.Core;

namespace StarConflictsRevolt.Tests.ClientTests;

public class BlazorProgramConfigurationTests
{
    [Test]
    public async Task BlazorProgram_ServiceRegistration_MatchesExpectedConfiguration()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        
        // Simulate the exact configuration from Blazor Program.cs
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        // Add shared client services
        builder.Services.Configure<GameClientConfiguration>(
            builder.Configuration.GetSection("GameClientConfiguration"));
        builder.Services.AddSingleton<ISignalRService, SignalRService>();

        // Register HTTP client with proper configuration
        builder.Services.AddHttpClient("GameApi", client =>
        {
            var apiBaseUrl = builder.Configuration["GameClientConfiguration:ApiBaseUrl"];
            if (!string.IsNullOrEmpty(apiBaseUrl))
            {
                client.BaseAddress = new Uri(apiBaseUrl);
            }
        });
        builder.Services.AddScoped<IHttpApiClient>(provider =>
        {
            var factory = provider.GetRequiredService<IHttpClientFactory>();
            return new HttpApiClient(factory, "GameApi");
        });

        // Add Blazor-specific services
        builder.Services.AddScoped<IGameStateService, GameStateService>();
        builder.Services.AddScoped<BlazorSignalRService>();

        // Act
        var app = builder.Build();

        // Assert - Verify all services are registered correctly
        using var scope = app.Services.CreateScope();
        var serviceProvider = scope.ServiceProvider;

        // Test service resolution
        var httpApiClient = serviceProvider.GetRequiredService<IHttpApiClient>();
        var signalRService = serviceProvider.GetRequiredService<ISignalRService>();
        var gameStateService = serviceProvider.GetRequiredService<IGameStateService>();
        var blazorSignalRService = serviceProvider.GetRequiredService<BlazorSignalRService>();

        await Assert.That(httpApiClient).IsNotNull();
        await Assert.That(signalRService).IsNotNull();
        await Assert.That(gameStateService).IsNotNull();
        await Assert.That(blazorSignalRService).IsNotNull();

        // Verify service types are correct (basic type checking)
        await Assert.That(httpApiClient is HttpApiClient).IsTrue();
        await Assert.That(signalRService is SignalRService).IsTrue();
        await Assert.That(gameStateService is GameStateService).IsTrue();
        await Assert.That(blazorSignalRService is BlazorSignalRService).IsTrue();
    }

    [Test]
    public async Task BlazorProgram_HttpClientConfiguration_IsCorrect()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        
        // Set up configuration
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["GameClientConfiguration:ApiBaseUrl"] = "http://localhost:5000",
            ["GameClientConfiguration:GameServerHubUrl"] = "http://localhost:5000/gamehub",
            ["GameClientConfiguration:GameServerUrl"] = "http://localhost:5000"
        });

        // Register HTTP client
        builder.Services.AddHttpClient("GameApi", client =>
        {
            var apiBaseUrl = builder.Configuration["GameClientConfiguration:ApiBaseUrl"];
            if (!string.IsNullOrEmpty(apiBaseUrl))
            {
                client.BaseAddress = new Uri(apiBaseUrl);
            }
        });

        // Act
        var app = builder.Build();
        using var scope = app.Services.CreateScope();
        var httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();
        var httpClient = httpClientFactory.CreateClient("GameApi");

        // Assert
        await Assert.That(httpClient.BaseAddress).IsEqualTo(new Uri("http://localhost:5000"));
    }

    [Test]
    public async Task BlazorProgram_ServiceLifetimes_AreCorrect()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        
        // Register services with the same configuration as Program.cs
        builder.Services.Configure<GameClientConfiguration>(
            builder.Configuration.GetSection("GameClientConfiguration"));
        builder.Services.AddSingleton<ISignalRService, SignalRService>();
        builder.Services.AddHttpClient("GameApi", client => { });
        builder.Services.AddScoped<IHttpApiClient>(provider =>
        {
            var factory = provider.GetRequiredService<IHttpClientFactory>();
            return new HttpApiClient(factory, "GameApi");
        });
        builder.Services.AddScoped<IGameStateService, GameStateService>();
        builder.Services.AddScoped<BlazorSignalRService>();

        // Act
        var app = builder.Build();

        // Assert - Test service lifetimes
        using var scope1 = app.Services.CreateScope();
        using var scope2 = app.Services.CreateScope();

        var signalRService1 = scope1.ServiceProvider.GetRequiredService<ISignalRService>();
        var signalRService2 = scope2.ServiceProvider.GetRequiredService<ISignalRService>();
        
        // Singleton should be the same instance
        await Assert.That(signalRService1).IsEqualTo(signalRService2);

        var gameStateService1 = scope1.ServiceProvider.GetRequiredService<IGameStateService>();
        var gameStateService2 = scope2.ServiceProvider.GetRequiredService<IGameStateService>();
        
        // Scoped should be different instances
        await Assert.That(gameStateService1).IsNotEqualTo(gameStateService2);
    }

    [Test]
    public async Task BlazorProgram_ConfigurationBinding_WorksCorrectly()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        
        // Set up configuration
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["GameClientConfiguration:ApiBaseUrl"] = "http://test-server:8080",
            ["GameClientConfiguration:GameServerHubUrl"] = "http://test-server:8080/hub",
            ["GameClientConfiguration:GameServerUrl"] = "http://test-server:8080"
        });

        builder.Services.Configure<GameClientConfiguration>(
            builder.Configuration.GetSection("GameClientConfiguration"));

        // Act
        var app = builder.Build();
        using var scope = app.Services.CreateScope();
        var config = scope.ServiceProvider.GetRequiredService<IOptions<GameClientConfiguration>>();

        // Assert
        await Assert.That(config.Value.ApiBaseUrl).IsEqualTo("http://test-server:8080");
        await Assert.That(config.Value.GameServerHubUrl).IsEqualTo("http://test-server:8080/hub");
        await Assert.That(config.Value.GameServerUrl).IsEqualTo("http://test-server:8080");
    }

    [Test]
    public async Task BlazorProgram_ConfigurationHandling_WorksWithValidUrls()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        
        // Set up valid configuration
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["GameClientConfiguration:ApiBaseUrl"] = "http://localhost:5000"
        });

        builder.Services.Configure<GameClientConfiguration>(
            builder.Configuration.GetSection("GameClientConfiguration"));

        builder.Services.AddHttpClient("GameApi", client =>
        {
            var apiBaseUrl = builder.Configuration["GameClientConfiguration:ApiBaseUrl"];
            if (!string.IsNullOrEmpty(apiBaseUrl))
            {
                client.BaseAddress = new Uri(apiBaseUrl);
            }
        });

        // Act & Assert
        var app = builder.Build();
        using var scope = app.Services.CreateScope();
        var httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();
        var httpClient = httpClientFactory.CreateClient("GameApi");
        
        await Assert.That(httpClient.BaseAddress).IsEqualTo(new Uri("http://localhost:5000"));
    }
}

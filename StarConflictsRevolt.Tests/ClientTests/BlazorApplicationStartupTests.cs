using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using StarConflictsRevolt.Clients.Blazor.Services;
using StarConflictsRevolt.Clients.Shared.Communication;
using StarConflictsRevolt.Clients.Shared.Http;
using StarConflictsRevolt.Clients.Models;
using TUnit.Assertions.AssertConditions.Throws;
using StarConflictsRevolt.Clients.Blazor.Components.Pages;

namespace StarConflictsRevolt.Tests.ClientTests;

public class BlazorApplicationStartupTests
{
    [Test]
    public async Task BlazorApplication_CanStart_WithoutRuntimeErrors()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        
        // Configure the same way as Blazor Program.cs
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        // Add shared client services
        builder.Services.Configure<GameClientConfiguration>(config =>
        {
            config.ApiBaseUrl = "http://localhost:5000";
            config.GameServerHubUrl = "http://localhost:5000/gamehub";
        });
        builder.Services.AddSingleton<ISignalRService, SignalRService>();

        // Register HTTP client with proper configuration
        builder.Services.AddHttpClient("GameApi", client =>
        {
            client.BaseAddress = new Uri("http://localhost:5000");
        });
        builder.Services.AddScoped<IHttpApiClient>(provider =>
        {
            var factory = provider.GetRequiredService<IHttpClientFactory>();
            return new HttpApiClient(factory, "GameApi");
        });

        // Add Blazor-specific services
        builder.Services.AddScoped<IGameStateService, GameStateService>();
        builder.Services.AddScoped<BlazorSignalRService>();

        // Act & Assert - This should not throw any DI exceptions
        var app = builder.Build();
        
        // Test that all services can be resolved
        using var scope = app.Services.CreateScope();
        var httpApiClient = scope.ServiceProvider.GetRequiredService<IHttpApiClient>();
        var signalRService = scope.ServiceProvider.GetRequiredService<ISignalRService>();
        var gameStateService = scope.ServiceProvider.GetRequiredService<IGameStateService>();
        var blazorSignalRService = scope.ServiceProvider.GetRequiredService<BlazorSignalRService>();
        
        // Verify services are properly configured
        await Assert.That(httpApiClient).IsNotNull();
        await Assert.That(signalRService).IsNotNull();
        await Assert.That(gameStateService).IsNotNull();
        await Assert.That(blazorSignalRService).IsNotNull();
    }

    [Test]
    public async Task BlazorApplication_ThrowsException_WhenHttpApiClientNotProperlyConfigured()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();

        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        // Try to register HttpApiClient without proper HTTP client configuration
        builder.Services.AddHttpClient<IHttpApiClient, HttpApiClient>(); // This should fail

        // Act & Assert
        var app = builder.Build();

        // This should throw a DI exception when trying to resolve services
        await Assert.That(() =>
        {
            using var scope = app.Services.CreateScope();
            scope.ServiceProvider.GetRequiredService<IHttpApiClient>();
        }).Throws<InvalidOperationException>().WithMessageContaining("A suitable constructor for type 'StarConflictsRevolt.Clients.Shared.Http.HttpApiClient' could not be located");
    }

    [Test]
    public async Task BlazorApplication_ThrowsException_WhenServicesHaveCircularDependencies()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();

        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        // Create a circular dependency scenario
        builder.Services.AddScoped<ServiceA>();
        builder.Services.AddScoped<ServiceB>();
        builder.Services.AddScoped<ServiceC>();

        // Act & Assert
        var app = builder.Build();

        // This should throw a DI exception due to circular dependency
        await Assert.That(() =>
        {
            using var scope = app.Services.CreateScope();
            scope.ServiceProvider.GetRequiredService<ServiceA>();
        }).Throws<InvalidOperationException>()
            .WithMessageContaining("circular dependency");
    }

    [Test]
    public async Task BlazorApplication_CanRenderComponents_WithoutErrors()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        // Add minimal required services for component rendering
        builder.Services.Configure<GameClientConfiguration>(config =>
        {
            config.ApiBaseUrl = "http://localhost:5000";
            config.GameServerHubUrl = "http://localhost:5000/gamehub";
            config.GameServerUrl = "http://localhost:5000";
        });
        
        builder.Services.AddSingleton<ISignalRService, SignalRService>();
        builder.Services.AddHttpClient("GameApi", client =>
        {
            client.BaseAddress = new Uri("http://localhost:5000");
        });
        builder.Services.AddScoped<IHttpApiClient>(provider =>
        {
            var factory = provider.GetRequiredService<IHttpClientFactory>();
            return new HttpApiClient(factory, "GameApi");
        });
        builder.Services.AddScoped<IGameStateService, GameStateService>();

        var app = builder.Build();

        // Act & Assert - Test that components can be instantiated
        using var scope = app.Services.CreateScope();
        var serviceProvider = scope.ServiceProvider;
        
        // This should not throw any exceptions
        var homeComponent = ActivatorUtilities.CreateInstance<Home>(serviceProvider);
        await Assert.That(homeComponent).IsNotNull();
    }

    [Test]
    public async Task BlazorApplication_ThrowsException_WhenRequiredServicesMissing()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();

        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        // Register only some services, missing others
        builder.Services.Configure<GameClientConfiguration>(config =>
        {
            config.ApiBaseUrl = "http://localhost:5000";
        });

        // Missing ISignalRService and IHttpApiClient

        var app = builder.Build();

        // Act & Assert
        using var scope = app.Services.CreateScope();
        var serviceProvider = scope.ServiceProvider;

        // This should throw when trying to create a component that needs missing services
        await Assert.That(() => ActivatorUtilities.CreateInstance<Home>(serviceProvider))
            .Throws<InvalidOperationException>()
            .WithMessageContaining("Unable to resolve service");
    }
}

// Test classes for circular dependency testing
public class ServiceA
{
    public ServiceA(ServiceB serviceB) { }
}

public class ServiceB
{
    public ServiceB(ServiceC serviceC) { }
}

public class ServiceC
{
    public ServiceC(ServiceA serviceA) { }
}

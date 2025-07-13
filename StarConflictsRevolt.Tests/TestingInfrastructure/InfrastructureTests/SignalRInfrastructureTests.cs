using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace StarConflictsRevolt.Tests.TestingInfrastructure.InfrastructureTests;

public class SignalRInfrastructureTests
{
    [Test]
    public async Task SignalR_Client_Can_Connect_And_Echo()
    {
        // Arrange: Start a minimal in-memory SignalR server
        var builder = WebApplication.CreateBuilder();
        builder.Logging.ClearProviders();
        builder.WebHost.UseUrls("http://127.0.0.1:0"); // Use dynamic port, valid for Kestrel
        builder.Services.AddSignalR();
        var app = builder.Build();
        app.MapHub<TestHub>("/testhub");
        await app.StartAsync();
        var url = app.Urls.First(u => u.StartsWith("http://")) + "/testhub";

        // Act: Connect a SignalR client
        var connection = new HubConnectionBuilder()
            .WithUrl(url)
            .WithAutomaticReconnect()
            .Build();
        await connection.StartAsync();
        var testMessage = "Hello, SignalR!";
        var response = await connection.InvokeAsync<string>("Echo", testMessage);

        // Assert
        await Assert.That(response).IsEqualTo(testMessage);

        // Cleanup
        await connection.StopAsync();
        await connection.DisposeAsync();
        await app.StopAsync();
        await app.DisposeAsync();
    }

    public class TestHub : Hub
    {
        public async Task<string> Echo(string message)
        {
            return await Task.FromResult(message);
        }
    }
}
using System.Net;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Events;

namespace StarConflictsRevolt.Tests.ServerTests.UnitTests;

public class ApiIntegrationTests
{
    [Test]
    public async Task MoveFleetCommand_Api_Returns202()
    {
        // Arrange: configure the API in-memory (minimal, for demo)
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();
        app.MapPost("/game/move-fleet", async context =>
        {
            var moveEvent = await JsonSerializer.DeserializeAsync<MoveFleetEvent>(context.Request.Body, cancellationToken: context.RequestAborted);
            if (moveEvent == null)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Invalid MoveFleetEvent");
                return;
            }

            context.Response.StatusCode = 202;
        });
        var port = GetRandomUnusedPort();
        app.Urls.Add($"http://localhost:{port}");
        await app.StartAsync();
        var client = new HttpClient { BaseAddress = new Uri($"http://localhost:{port}") };

        // Act
        var moveEventObj = new MoveFleetEvent(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        var response = await client.PostAsJsonAsync("/game/move-fleet", moveEventObj);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Accepted);

        await app.StopAsync();
    }

    private static int GetRandomUnusedPort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }
}

// Minimal in-memory event store for test DI
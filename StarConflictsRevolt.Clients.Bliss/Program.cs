using Microsoft.Extensions.Hosting;
using StarConflictsRevolt.Clients.Bliss.Core;
using StarConflictsRevolt.Clients.Bliss.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace StarConflictsRevolt.Clients.Bliss;

/// <summary>
/// Main entry point for the Bliss client.
/// </summary>
public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        // Add Bliss client services
        builder.Services.AddBlissClientServices(builder.Configuration);

        var host = builder.Build();

        try
        {
            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Starting Star Conflicts Revolt Bliss Client");

            // Get the game loop and run it
            var gameLoop = host.Services.GetRequiredService<IGameLoop>();
            await gameLoop.RunAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while running the Bliss client");
            throw;
        }
    }
}
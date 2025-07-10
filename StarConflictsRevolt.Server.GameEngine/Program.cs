using StarConflictsRevolt.Server.GameEngine;
using StarConflictsRevolt.Aspire.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

// Add ServiceDefaults for service discovery and observability
builder.AddServiceDefaults();

GameEngineStartupHelper.RegisterGameEngineServices(builder);
GameEngineStartupHelper.RegisterGameEngineDbContext(builder);
GameEngineStartupHelper.RegisterGameEngineDocumentStore(builder);

var app = builder.Build();

// Ensure database is created with retry logic
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<StarConflictsRevolt.Server.Datastore.GameDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    
    // Log the connection string being used (without sensitive info)
    var connectionString = configuration.GetConnectionString("gameDb");
    if (!string.IsNullOrEmpty(connectionString))
    {
        var safeConnectionString = connectionString.Replace("Password=", "Password=***");
        logger.LogInformation("Using connection string: {ConnectionString}", safeConnectionString);
    }
    else
    {
        logger.LogWarning("No connection string found for 'gameDb'");
    }
    
    var maxRetries = 5;
    var retryDelay = TimeSpan.FromSeconds(1);
    
    for (int attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            logger.LogInformation("Attempting to ensure database is created (attempt {Attempt}/{MaxRetries})", attempt, maxRetries);
            db.Database.EnsureCreated();
            logger.LogInformation("Database created successfully");
            break;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to create database on attempt {Attempt}/{MaxRetries}", attempt, maxRetries);
            
            if (attempt == maxRetries)
            {
                logger.LogError(ex, "Failed to create database after {MaxRetries} attempts. Application will continue but database operations may fail.", maxRetries);
                break;
            }
            
            Thread.Sleep(retryDelay);
        }
    }
}

GameEngineStartupHelper.ConfigureGameEngine(app);

app.Run();

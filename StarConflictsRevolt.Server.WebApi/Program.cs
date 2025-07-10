
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using StarConflictsRevolt.Server.WebApi;
using StarConflictsRevolt.Aspire.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

// Add ServiceDefaults first for proper service discovery and observability
builder.AddServiceDefaults();

// Add JWT authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidIssuer = JwtConfig.Issuer,
            ValidAudience = JwtConfig.Audience,
            IssuerSigningKey = JwtConfig.GetSymmetricSecurityKey(),
        };
    });

// Add API versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});

WebApiStartupHelper.RegisterServices(builder);
WebApiStartupHelper.RegisterGameEngineDbContext(builder);
WebApiStartupHelper.RegisterRavenDb(builder);

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

// Enable authentication/authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Add a simple health check endpoint
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }));

WebApiStartupHelper.Configure(app);

app.Run();
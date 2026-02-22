using StarConflictsRevolt.Aspire.ServiceDefaults;
using StarConflictsRevolt.Server.WebApi.Infrastructure.Configuration;
using StarConflictsRevolt.Server.WebApi.Infrastructure.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

StartupHelper.RegisterRavenDb(builder);  // Before RegisterAllServices so IEventStore is available

// Dependency health checks for Aspire dashboard (RavenDB; Redis not used by WebApi yet)
builder.Services.AddHealthChecks()
    .AddCheck<RavenDbHealthCheck>("ravendb", tags: ["ready"]);
StartupHelper.RegisterAllServices(builder);
StartupHelper.RegisterLiteDb(builder);

var app = builder.Build();

app.UseHttpsRedirection();
if (!app.Environment.IsDevelopment())
    app.UseHsts();

var config = app.Services.GetRequiredService<IConfiguration>();
var logger = app.Services.GetRequiredService<ILogger<Program>>();
if (config.GetConnectionString("ravenDb") == "SET_BY_ASPIRE_OR_ENVIRONMENT")
    logger.LogWarning("ravenDb connection string not set (Aspire containers may not be running). Start Docker and run AppHost again, or set ConnectionStrings__ravenDb.");

app.MapDefaultEndpoints();

await StartupHelper.ConfigureAsync(app);

await app.RunAsync();
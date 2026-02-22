using StarConflictsRevolt.Aspire.ServiceDefaults;
using StarConflictsRevolt.Server.WebApi.Infrastructure.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

StartupHelper.RegisterAllServices(builder);
StartupHelper.RegisterRavenDb(builder);
StartupHelper.RegisterGameDbContext(builder);
StartupHelper.RegisterTelemetry(builder);

var app = builder.Build();

app.UseHttpsRedirection();
if (!app.Environment.IsDevelopment())
    app.UseHsts();

var config = app.Services.GetRequiredService<IConfiguration>();
var logger = app.Services.GetRequiredService<ILogger<Program>>();
if (config.GetConnectionString("gameDb") == "SET_BY_ASPIRE_OR_ENVIRONMENT")
    logger.LogWarning("gameDb connection string not set (Aspire containers may not be running). Start Docker and run AppHost again, or set ConnectionStrings__gameDb.");
if (config.GetConnectionString("ravenDb") == "SET_BY_ASPIRE_OR_ENVIRONMENT")
    logger.LogWarning("ravenDb connection string not set (Aspire containers may not be running). Start Docker and run AppHost again, or set ConnectionStrings__ravenDb.");

app.MapDefaultEndpoints();

await StartupHelper.ConfigureAsync(app);

await app.RunAsync();
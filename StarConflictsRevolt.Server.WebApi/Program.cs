using StarConflictsRevolt.Aspire.ServiceDefaults;
using StarConflictsRevolt.Server.WebApi.Helpers;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

StartupHelper.RegisterAllServices(builder);
StartupHelper.RegisterRavenDb(builder);
StartupHelper.RegisterGameDbContext(builder);

var app = builder.Build();

var config = app.Services.GetRequiredService<IConfiguration>();
System.Diagnostics.Debug.Assert(config.GetConnectionString("gameDb") != "SET_BY_ASPIRE_OR_ENVIRONMENT", "Aspire did not override gameDb connection string");
System.Diagnostics.Debug.Assert(config.GetConnectionString("ravenDb") != "SET_BY_ASPIRE_OR_ENVIRONMENT", "Aspire did not override ravenDb connection string");

app.MapDefaultEndpoints();

StartupHelper.Configure(app);

app.Run();
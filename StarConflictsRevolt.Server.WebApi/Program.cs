using StarConflictsRevolt.Server.WebApi;
using StarConflictsRevolt.Aspire.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

// Add ServiceDefaults first for proper service discovery and observability
builder.AddServiceDefaults();

WebApiStartupHelper.RegisterServices(builder);
WebApiStartupHelper.RegisterGameEngineDbContext(builder);
WebApiStartupHelper.RegisterRavenDb(builder);

var app = builder.Build();

WebApiStartupHelper.Configure(app);

app.Run();
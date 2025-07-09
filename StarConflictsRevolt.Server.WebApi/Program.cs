using StarConflictsRevolt.Server.WebApi;

var builder = WebApplication.CreateBuilder(args);

WebApiStartupHelper.RegisterServices(builder);
WebApiStartupHelper.RegisterGameEngineDbContext(builder);
WebApiStartupHelper.RegisterRavenDb(builder);

var app = builder.Build();

WebApiStartupHelper.Configure(app);

app.Run();
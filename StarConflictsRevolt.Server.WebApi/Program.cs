using StarConflictsRevolt.Server.WebApi;
using StarConflictsRevolt.Server.WebApi.Helpers;

var builder = WebApplication.CreateBuilder(args);
StartupHelper.RegisterAllServices(builder);
StartupHelper.RegisterRavenDb(builder);
StartupHelper.RegisterGameDbContext(builder);

var app = builder.Build();
StartupHelper.Configure(app);

app.Run();
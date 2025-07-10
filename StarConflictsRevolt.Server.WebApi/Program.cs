using StarConflictsRevolt.Server.WebApi;
using StarConflictsRevolt.Server.WebApi.Helpers;

var builder = WebApplication.CreateBuilder(args);
StartupHelper.RegisterAllServices(builder);
StartupHelper.RegisterRavenDb(builder);
StartupHelper.RegisterGameDbContext(builder);

var app = builder.Build();

#if DEBUG
System.Diagnostics.Debug.Assert(!string.IsNullOrWhiteSpace(app.Services.GetRequiredService<IConfiguration>().GetConnectionString("gameDb")), "gameDb connection string should not be empty");
System.Diagnostics.Debug.Assert(!string.IsNullOrWhiteSpace(app.Services.GetRequiredService<IConfiguration>().GetConnectionString("ravenDb")), "ravenDb connection string should not be empty");
#endif

StartupHelper.Configure(app);

app.Run();
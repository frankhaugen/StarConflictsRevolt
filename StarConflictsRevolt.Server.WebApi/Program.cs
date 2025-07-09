var builder = WebApplication.CreateBuilder(args);

WebApiStartupHelper.RegisterServices(builder);

var app = builder.Build();

WebApiStartupHelper.Configure(app);

app.Run();
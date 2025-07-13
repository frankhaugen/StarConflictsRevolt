using StarConflictsRevolt.Aspire.ServiceDefaults;
using StarConflictsRevolt.Clients.Raylib.Services;

var builder = Host.CreateApplicationBuilder(args);

// Add ServiceDefaults for service discovery and observability
builder.AddServiceDefaults();

// Configure services
builder.AddClientServices();

// Build and run
var host = builder.Build();

// Initialize client
var clientInitializer = host.Services.GetRequiredService<IClientInitializer>();
await clientInitializer.InitializeAsync();

await host.RunAsync();
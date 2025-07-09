using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Add HTTP client with service discovery for Blazor WebAssembly
builder.Services.AddHttpClient();

await builder.Build().RunAsync();
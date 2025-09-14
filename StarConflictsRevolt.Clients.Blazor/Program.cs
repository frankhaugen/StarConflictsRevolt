using StarConflictsRevolt.Clients.Blazor.Components;
using StarConflictsRevolt.Clients.Shared;
using StarConflictsRevolt.Clients.Blazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add shared client services
builder.Services.AddSharedClientServices(builder.Configuration);

// Add Blazor-specific services
builder.Services.AddScoped<IGameStateService, GameStateService>();
builder.Services.AddScoped<ISignalRService, BlazorSignalRService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

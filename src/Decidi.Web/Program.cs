using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Decidi.Web;
using Decidi.Web.Auth;
using Decidi.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:5000";

builder.Services.AddSingleton<ToastService>();
builder.Services.AddSingleton<LoadingService>();

builder.Services.AddScoped<JwtAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<JwtAuthStateProvider>());
builder.Services.AddScoped<GlobalHttpHandler>();

builder.Services.AddHttpClient("Decidi.API", client =>
    {
        client.BaseAddress = new Uri(apiBaseUrl);
    })
    .AddHttpMessageHandler<GlobalHttpHandler>();

builder.Services.AddHttpClient("Decidi.API.NoAuth", client =>
    {
        client.BaseAddress = new Uri(apiBaseUrl);
    });

builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IHttpClientFactory>().CreateClient("Decidi.API"));

builder.Services.AddScoped<ApiClient>();
builder.Services.AddScoped<AppState>();
builder.Services.AddAuthorizationCore();

await builder.Build().RunAsync();

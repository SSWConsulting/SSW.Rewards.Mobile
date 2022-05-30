using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Net.Http;
using SSW.Rewards.Admin;
using SSW.Rewards.Admin.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// MessageHandler for adding the JWT to outbound requests to the API
builder.Services.AddTransient<CustomAuthorizationMessageHandler>();

string? apiBaseUrl = builder.Configuration["RewardsApiUrl"];
// TODO: Move this somewhere better.
string httpClientName = "WebAPI";
if (apiBaseUrl == null)
{
    throw new NullReferenceException("No API base URL provided");
}
// Http Client
builder.Services.AddHttpClient(httpClientName, client => client.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler(sp => sp.GetRequiredService<CustomAuthorizationMessageHandler>());

// Services
builder.Services.AddScoped<AchievementsService>();
builder.Services.AddScoped<LeaderboardService>();
builder.Services.AddScoped<RewardsService>();
builder.Services.AddScoped<StaffService>();


string[] authScopes = builder.Configuration.GetSection("Local:Scopes").Get<string[]>();

builder.Services.AddOidcAuthentication(options =>
{
    
    builder.Configuration.Bind("Local", options.ProviderOptions);
    
    foreach(var scope in authScopes)
    {
        options.ProviderOptions.DefaultScopes.Add(scope);
    }
    options.ProviderOptions.ResponseType = "code";
});

await builder.Build().RunAsync();

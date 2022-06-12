using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SSW.Rewards.Admin;
using MudBlazor.Services;
using SSW.Rewards.Admin.Services;
using SSW.Rewards.Api;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// MessageHandler for adding the JWT to outbound requests to the API
builder.Services.AddTransient<CustomAuthorizationMessageHandler>();

string? apiBaseUrl = builder.Configuration["RewardsApiUrl"];
if (apiBaseUrl == null)
{
    throw new NullReferenceException("No API base URL provided");
}

// Http Client
builder.Services.AddHttpClient(Constants.RewardsApiClient, client => client.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler(sp => sp.GetRequiredService<CustomAuthorizationMessageHandler>());

// Services
builder.Services.AddScoped<AchievementsService>();
builder.Services.AddScoped<LeaderboardService>();
builder.Services.AddScoped<NotificationsService>();
builder.Services.AddScoped<RewardsService>();
builder.Services.AddScoped<StaffService>();
builder.Services.AddScoped<SkillService>();

builder.Services.AddMudServices();

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

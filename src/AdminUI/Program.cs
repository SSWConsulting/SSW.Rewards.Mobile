using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor;
using MudBlazor.Services;
using SSW.Rewards.Admin.UI.Services;
using SSW.Rewards.ApiClient;
using SSW.Rewards.Application.Common.Interfaces;

namespace SSW.Rewards.Admin.UI;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        // MessageHandler for adding the JWT to outbound requests to the API
        builder.Services.AddTransient<CustomAuthorizationMessageHandler>();

        builder.Services.AddTransient<IDateTime, DateTimeService>();

        builder.Services.AddScoped<IPrizeDrawManager, PrizeDrawManager>();
        builder.Services.AddScoped<IWinnerStorageService, WinnerStorageService>();

        string? identityUrl = builder.Configuration["Local:Authority"];
        if (identityUrl == null)
        {
            throw new NullReferenceException("No Authority URL provided");
        }

        const string identityClientName = "IdentityClient";
        builder.Services.AddHttpClient(
            identityClientName,
            o =>
            {
                o.BaseAddress = new Uri(identityUrl);
                o.DefaultRequestHeaders.Add("Accept", "application/json");
            })
            .AddHttpMessageHandler(sp => sp.GetRequiredService<CustomAuthorizationMessageHandler>());

        string? apiBaseUrl = builder.Configuration["RewardsApiUrl"];
        if (apiBaseUrl == null)
        {
            throw new NullReferenceException("No API base URL provided");
        }

        builder.Services.AddApiClientServices<CustomAuthorizationMessageHandler>(apiBaseUrl, true);

        builder.Services.AddMudServices(config =>
        {
            config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
            config.SnackbarConfiguration.PreventDuplicates = true;
            config.SnackbarConfiguration.NewestOnTop = false;
            config.SnackbarConfiguration.ShowCloseIcon = true;
            config.SnackbarConfiguration.VisibleStateDuration = 2000;
            config.SnackbarConfiguration.HideTransitionDuration = 500;
            config.SnackbarConfiguration.ShowTransitionDuration = 500;
            config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
        });

        string[] authScopes = builder.Configuration.GetSection("Local:Scopes").Get<string[]>() ?? Array.Empty<string>();

        builder.Services.AddOidcAuthentication(options =>
        {
            builder.Configuration.Bind("Local", options.ProviderOptions);

            foreach (var scope in authScopes)
            {
                options.ProviderOptions.DefaultScopes.Add(scope);
            }

            options.ProviderOptions.ResponseType = "code";
        })
#if !DEBUG
        // PRODUCTION: Use standard RolesClaimsPrincipalFactory that processes JWT role claims only
        .AddAccountClaimsPrincipalFactory<RolesClaimsPrincipalFactory>();
#else
        // DEBUG: Use WebRolesClaimsPrincipalFactory to fetch roles from database
        // This is a workaround for SSW.Identity limitation where @ssw.com.au email addresses
        // don't receive proper role claims in JWT tokens. This needs to be fixed in SSW.Identity.
        // See: WebRolesClaimsPrincipalFactory.cs for detailed documentation.
        .AddAccountClaimsPrincipalFactory<WebRolesClaimsPrincipalFactory>();
        
        Console.WriteLine("[DEBUG] Using WebRolesClaimsPrincipalFactory - roles will be fetched from database");
        Console.WriteLine("[DEBUG] This is a workaround for only @ssw.com.au email getting proper roles assigned");
#endif

        await builder.Build().RunAsync();
    }
}
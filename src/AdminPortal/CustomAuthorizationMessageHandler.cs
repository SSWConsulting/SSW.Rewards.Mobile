using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace SSW.Rewards.Admin.UI;
public class CustomAuthorizationMessageHandler : AuthorizationMessageHandler
{
    private readonly IConfiguration _configuration;

    public CustomAuthorizationMessageHandler(
        IAccessTokenProvider provider,
        IConfiguration config,
        NavigationManager navigationManager)
        : base(provider, navigationManager)
    {
        _configuration = config;
        ConfigureHandler(
            authorizedUrls: new[] { _configuration.GetValue<string>("RewardsApiUrl") },
            scopes: new[] { "email", "profile", "ssw-rewards-api" });
    }
}
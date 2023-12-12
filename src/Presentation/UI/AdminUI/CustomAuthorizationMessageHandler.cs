using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace SSW.Rewards.Admin.UI;
public class CustomAuthorizationMessageHandler : AuthorizationMessageHandler
{
    public CustomAuthorizationMessageHandler(
        IAccessTokenProvider provider,
        IConfiguration config,
        NavigationManager navigationManager)
        : base(provider, navigationManager)
    {
        ConfigureHandler(
            authorizedUrls: new[] { config.GetValue<string>("RewardsApiUrl") } !,
            scopes: new[] { "email", "profile", "ssw-rewards-api" });
    }
}
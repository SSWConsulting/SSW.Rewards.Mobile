using IdentityModel.OidcClient.Browser;
using IBrowser = IdentityModel.OidcClient.Browser.IBrowser;

namespace SSW.Rewards.Mobile.Helpers;

public class AuthBrowser : IBrowser
{
    public async Task<BrowserResult> InvokeAsync(BrowserOptions options, CancellationToken cancellationToken = default)
    {
        WebAuthenticatorResult authResult = await WebAuthenticator.AuthenticateAsync(new Uri(options.StartUrl), new Uri(Constants.AuthRedirectUrl));

        return new BrowserResult
        {
            Response = ParseAuthenticationResult(authResult)
        };
    }

    private string ParseAuthenticationResult(WebAuthenticatorResult result)
    {
        var code = String.Empty;
        var state = String.Empty;
        var scope = String.Empty;
        var sessionState = String.Empty;
        result?.Properties.TryGetValue("code", out code);
        result?.Properties.TryGetValue("state", out state);
        result?.Properties.TryGetValue("scope", out scope);
        result?.Properties.TryGetValue("session_state", out sessionState);

        return $"{Constants.AuthRedirectUrl}#code={code}&scope={scope}&state={state}&session_state={sessionState}";
    }
}

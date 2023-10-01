using System.Net.Http.Headers;

namespace SSW.Rewards.Mobile.Helpers;

public class AuthHandler : DelegatingHandler
{
    private static string AuthToken;

    public const string AuthenticatedClient = "AuthenticatedClient";

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AuthToken);

        return await base.SendAsync(request, cancellationToken);
    }

    public static void SetAccessToken(string token) => AuthToken = token;
}
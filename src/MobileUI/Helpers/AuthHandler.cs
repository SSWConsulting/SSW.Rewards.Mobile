using System.Net.Http.Headers;

namespace SSW.Rewards.Mobile.Helpers;

public class AuthHandler : DelegatingHandler
{
    private readonly IAuthenticationService _authenticationService;
    public AuthHandler(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _authenticationService.GetAccesstoken();
        
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await base.SendAsync(request, cancellationToken);
    }
}
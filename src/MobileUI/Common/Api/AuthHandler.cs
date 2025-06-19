using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;

namespace SSW.Rewards.Mobile.Helpers;

public class AuthHandler : DelegatingHandler
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ILogger<AuthHandler> _logger;

    public AuthHandler(IAuthenticationService authenticationService, ILogger<AuthHandler> logger)
    {
        _authenticationService = authenticationService;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            var token = await _authenticationService.GetAccessTokenAsync();

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                _logger.LogWarning("No valid token available for request to {Uri}", request.RequestUri);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving access token for request to {Uri}", request.RequestUri);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
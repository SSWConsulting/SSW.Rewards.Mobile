using Duende.IdentityModel.Client;
using Duende.IdentityModel.OidcClient;
using Duende.IdentityModel.OidcClient.Results;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SSW.Rewards.Models;
using IBrowser = Duende.IdentityModel.OidcClient.Browser.IBrowser;

namespace SSW.Rewards.Mobile.Services.Authentication;

public interface IOidcAuthenticationProvider
{
    Task<AuthResult> LoginAsync(bool promptLogin = false);
    Task<AuthResult> RefreshTokenAsync(string refreshToken);
    Task<AuthResult> SilentLoginAsync();
}

public class OidcAuthenticationProvider : IOidcAuthenticationProvider
{
    private readonly OidcClient _oidcClient;
    private readonly ILogger<OidcAuthenticationProvider> _logger;

    public OidcAuthenticationProvider(
        IBrowser browser, 
        IOptions<AuthenticationOptions> options,
        ILogger<OidcAuthenticationProvider> logger)
    {
        _logger = logger;

        var oidcOptions = new OidcClientOptions
        {
            Authority = options.Value.Authority,
            ClientId = options.Value.ClientId,
            Scope = options.Value.Scope,
            RedirectUri = options.Value.RedirectUri,
            Browser = browser,
        };

        _oidcClient = new OidcClient(oidcOptions);
    }

    public async Task<AuthResult> LoginAsync(bool promptLogin = false)
    {
        try
        {
            var loginRequest = new LoginRequest();

            if (promptLogin)
            {
                loginRequest.FrontChannelExtraParameters = new Parameters
                {
                    { "prompt", "login" }
                };
            }

            var result = await _oidcClient.LoginAsync(loginRequest);
            return MapToAuthResult(result);
        }
        catch (TaskCanceledException)
        {
            _logger.LogInformation("Login was cancelled by the user");
            return new AuthResult
            {
                Error = "cancelled",
                ErrorDescription = "User cancelled the login process"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during login");
            return new AuthResult
            {
                Error = "exception",
                ErrorDescription = ex.Message
            };
        }
    }

    public async Task<AuthResult> RefreshTokenAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            _logger.LogWarning("Refresh token is null or empty");
            return new AuthResult
            {
                Error = "invalid_token",
                ErrorDescription = "Refresh token is null or empty"
            };
        }

        try
        {
            var result = await _oidcClient.RefreshTokenAsync(refreshToken);
            return MapToAuthResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during token refresh");
            return new AuthResult
            {
                Error = "exception",
                ErrorDescription = ex.Message
            };
        }
    }

    public async Task<AuthResult> SilentLoginAsync()
    {
        try
        {
            var loginRequest = new LoginRequest
            {
                FrontChannelExtraParameters = new Parameters
                {
                    { "prompt", "none" }
                }
            };

            var result = await _oidcClient.LoginAsync(loginRequest);
            return MapToAuthResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Silent login failed");
            return new AuthResult
            {
                Error = "silent_login_failed",
                ErrorDescription = ex.Message
            };
        }
    }

    private AuthResult MapToAuthResult(LoginResult result)
    {
        if (result.IsError)
        {
            _logger.LogWarning("Login result contains error: {Error}, {ErrorDescription}", 
                result.Error, result.ErrorDescription);

            return new AuthResult
            {
                Error = result.Error,
                ErrorDescription = result.ErrorDescription
            };
        }

        return new AuthResult
        {
            AccessToken = result.AccessToken,
            RefreshToken = result.RefreshToken,
            IdentityToken = result.IdentityToken,
            AccessTokenExpiration = result.AccessTokenExpiration
        };
    }

    private AuthResult MapToAuthResult(RefreshTokenResult result)
    {
        if (result.IsError)
        {
            _logger.LogWarning("Token refresh result contains error: {Error}, {ErrorDescription}", 
                result.Error, result.ErrorDescription);

            return new AuthResult
            {
                Error = result.Error,
                ErrorDescription = result.ErrorDescription
            };
        }

        return new AuthResult
        {
            AccessToken = result.AccessToken,
            RefreshToken = result.RefreshToken,
            IdentityToken = result.IdentityToken,
            AccessTokenExpiration = result.AccessTokenExpiration
        };
    }
}

using System.IdentityModel.Tokens.Jwt;
using IdentityModel.Client;
using IdentityModel.OidcClient;
using IdentityModel.OidcClient.Results;
using Microsoft.AppCenter.Crashes;
using IBrowser = IdentityModel.OidcClient.Browser.IBrowser;

namespace SSW.Rewards.Mobile.Services;

public interface IAuthenticationService
{
    Task<ApiStatus> SignInAsync();
    Task<string> GetAccessToken();
    void SignOut();

    event EventHandler<DetailsUpdatedEventArgs> DetailsUpdated;
}

public class AuthenticationService : IAuthenticationService
{
    private readonly OidcClientOptions _options;

    private bool _loggedIn = false;

    private string RefreshToken;
    
    private string _accessToken;
    
    private DateTimeOffset _tokenExpiry;
    private DateTimeOffset _refreshTokenExpiry;

    public event EventHandler<DetailsUpdatedEventArgs> DetailsUpdated;

    private bool HasCachedAccount { get => Preferences.Get(nameof(HasCachedAccount), false); }
    
    public AuthenticationService(IBrowser browser)
    {
        _options = new OidcClientOptions
        {
            Authority = Constants.AuthorityUri,
            ClientId = Constants.ClientId,
            Scope = Constants.Scope,
            RedirectUri = Constants.AuthRedirectUrl,
            Browser = browser,
        };
    }
    
    public async Task<ApiStatus> SignInAsync()
    {
        try
        {
            var oidcClient = new OidcClient(_options);

            var result = await oidcClient.LoginAsync(new LoginRequest()
            {
                FrontChannelExtraParameters = HasCachedAccount ? null : new Parameters
                {
                    { "prompt", "login" }
                }
            });

            if (result.IsError)
            {
                SignOut();
                return ApiStatus.Error;
            }

            var authResult = GetAuthResult(result);

            await SettRefreshToken(authResult);

            return await SetLoggedInState(authResult);
        }
        catch (TaskCanceledException taskEx)
        {
            return ApiStatus.LoginFailure;
        }
        catch (Exception ex)
        {
            SignOut();
            return ApiStatus.Error;
        }
    }

    public async Task<string> GetAccessToken()
    {
        if (!string.IsNullOrWhiteSpace(_accessToken) && _tokenExpiry > DateTimeOffset.Now.AddMinutes(2))
        {
            return _accessToken;
        }

        if (await RefreshLoginAsync())
        {
            return _accessToken;
        }

        return string.Empty;
    }

    public void SignOut()
    {
        // TODO: remove from auth client
        SecureStorage.RemoveAll();
        Preferences.Clear();
    }

    private async Task<ApiStatus> SetLoggedInState(AuthResult loginResult)
    {
        if (!string.IsNullOrWhiteSpace(loginResult.IdentityToken) && !string.IsNullOrWhiteSpace(loginResult.AccessToken))
        {

            try
            {                
                Preferences.Set(nameof(HasCachedAccount), true);

                var tokenHandler = new JwtSecurityTokenHandler();

                var jwToken = tokenHandler.ReadJwtToken(loginResult.IdentityToken);

                var firstName = jwToken.Claims.FirstOrDefault(t => t.Type == "given_name")?.Value;
                var familyName = jwToken.Claims.FirstOrDefault(t => t.Type == "family_name")?.Value;
                var jobTitle = jwToken.Claims.FirstOrDefault(t => t.Type == "jobTitle")?.Value;
                var email = jwToken.Claims.FirstOrDefault(t => t.Type == "emails")?.Value;

                string fullName = firstName + " " + familyName;

                if (!string.IsNullOrWhiteSpace(fullName))
                {
                    Preferences.Set("MyName", fullName);
                }

                if (!string.IsNullOrWhiteSpace(jobTitle))
                {
                    Preferences.Set("JobTitle", jobTitle);
                }

                if (!string.IsNullOrWhiteSpace(email))
                {
                    Preferences.Set("MyEmail", email);
                }

                DetailsUpdated?.Invoke(this, new DetailsUpdatedEventArgs
                {
                    Name = fullName,
                    Email = email,
                    Jobtitle = jobTitle
                });

                _loggedIn = true;
            }
            catch (Exception ex)
            {
                return ApiStatus.Unavailable;
            }

            return ApiStatus.Success;
        }
        else
        {
            return ApiStatus.LoginFailure;
        }
    }

    private async Task SettRefreshToken(AuthResult result)
    {
        if (!string.IsNullOrWhiteSpace(RefreshToken))
        {
            RefreshToken = result.RefreshToken;
            await SecureStorage.SetAsync(nameof(RefreshToken), RefreshToken);
        }

        if (result.RefreshTokenExpiration.HasValue)
        {
            _refreshTokenExpiry = result.RefreshTokenExpiration.Value;
            Preferences.Set(nameof(_refreshTokenExpiry), _refreshTokenExpiry.ToUnixTimeSeconds());
        }
    }

    public async Task<bool> RefreshLoginAsync()
    {
        RefreshToken = await SecureStorage.GetAsync(nameof(RefreshToken));

        var refreshTokenExpiry = Preferences.Get(nameof(_refreshTokenExpiry), 0L);

        if (refreshTokenExpiry > 0)
        {
            _refreshTokenExpiry = DateTimeOffset.FromUnixTimeSeconds(refreshTokenExpiry);
        }

        if (!string.IsNullOrWhiteSpace(RefreshToken) && _refreshTokenExpiry > DateTimeOffset.Now.AddMinutes(2))
        {
            var oidcClient = new OidcClient(_options);

            var result = await oidcClient.RefreshTokenAsync(RefreshToken);

            if (!result.IsError)
            {
                var authResult = GetAuthResult(result);
                await SettRefreshToken(authResult);
                await SetLoggedInState(authResult);
                return true;
            }
            else
            {
                Crashes.TrackError(new Exception($"{result.Error}, {result.ErrorDescription}"));

                var fcep = new Parameters
                {
                    { "prompt", "none" }
                };

                var silentRequest = new LoginRequest
                {
                    FrontChannelExtraParameters = fcep
                };

                var silentResult = await oidcClient.LoginAsync(silentRequest);

                if (!silentResult.IsError)
                {
                    try
                    {
                        var authResult = GetAuthResult(silentResult);
                        await SetLoggedInState(authResult);

                        await SettRefreshToken(authResult);

                        return true;
                    }
                    catch
                    {

                        return false;
                    }
                }

                await SignInAsync();
            }
        }

        return false;
    }

    private AuthResult GetAuthResult<TResult> (TResult result)
    {
        if (result is LoginResult loginResult)
        {
            return new AuthResult
            {
                AccessToken = loginResult.AccessToken,
                RefreshToken = loginResult.RefreshToken,
                AccessTokenExpiration = loginResult.AccessTokenExpiration,
                IdentityToken = loginResult.IdentityToken,
                RefreshTokenExpiration = null
            };
        }
        else if (result is RefreshTokenResult refreshTokenResult)
        {
            return new AuthResult
            {
                AccessToken = refreshTokenResult.AccessToken,
                RefreshToken = refreshTokenResult.RefreshToken,
                IdentityToken = refreshTokenResult.IdentityToken,
                AccessTokenExpiration = refreshTokenResult.AccessTokenExpiration,
                RefreshTokenExpiration = DateTimeOffset.UtcNow.AddSeconds(refreshTokenResult.ExpiresIn)
            };
        }
        else
        {
            throw new ArgumentException("Unexpected result type", nameof(result));
        }
    }

    private class AuthResult
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string IdentityToken { get; set; }
        public DateTimeOffset AccessTokenExpiration { get; set; }
        public DateTimeOffset? RefreshTokenExpiration { get; set; }
    }
}

public class DetailsUpdatedEventArgs : EventArgs
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string Jobtitle { get; set; }
}
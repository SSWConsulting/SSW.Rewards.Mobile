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
            await SetRefreshToken(authResult);
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

    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
    public async Task<string> GetAccessToken()
    {
        if (!string.IsNullOrWhiteSpace(_accessToken) && _tokenExpiry > DateTimeOffset.Now.AddMinutes(2))
        {
            return _accessToken;
        }

        await _semaphore.WaitAsync();
        try
        {
            // Recheck the token after acquiring the lock to handle the case where
            // the token was refreshed while waiting for the semaphore.
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
        finally
        {
            _semaphore.Release();
        }
    }

    public void SignOut()
    {
        // TODO: remove from auth client
        SecureStorage.RemoveAll();
        Preferences.Clear();
        Preferences.Set("FirstRun", false);
    }

    private async Task<ApiStatus> SetLoggedInState(AuthResult loginResult)
    {
        if (!string.IsNullOrWhiteSpace(loginResult.IdentityToken) && !string.IsNullOrWhiteSpace(loginResult.AccessToken))
        {
            _accessToken = loginResult.AccessToken;
            _tokenExpiry = loginResult.AccessTokenExpiration;

            try
            {
                Preferences.Set(nameof(HasCachedAccount), true);

                var tokenHandler = new JwtSecurityTokenHandler();

                var jwToken = tokenHandler.ReadJwtToken(loginResult.IdentityToken);

                var firstName = jwToken.Claims.FirstOrDefault(t => t.Type == "given_name")?.Value;
                var familyName = jwToken.Claims.FirstOrDefault(t => t.Type == "family_name")?.Value;
                var jobTitle = jwToken.Claims.FirstOrDefault(t => t.Type == "jobTitle")?.Value;
                var email = jwToken.Claims.FirstOrDefault(t => t.Type == "email")?.Value;

                string fullName = firstName + " " + familyName;

                if (!string.IsNullOrWhiteSpace(jobTitle))
                {
                    Preferences.Set("JobTitle", jobTitle);
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

    private async Task SetRefreshToken(AuthResult result)
    {
        if (!string.IsNullOrWhiteSpace(result.RefreshToken))
        {
            RefreshToken = result.RefreshToken;
            await SecureStorage.SetAsync(nameof(RefreshToken), RefreshToken);
        }
    }

    public async Task<bool> RefreshLoginAsync()
    {
        RefreshToken = await SecureStorage.GetAsync(nameof(RefreshToken));

        if (!string.IsNullOrWhiteSpace(RefreshToken))
        {
            var oidcClient = new OidcClient(_options);

            var result = await oidcClient.RefreshTokenAsync(RefreshToken);

            if (!result.IsError)
            {
                var authResult = GetAuthResult(result);
                await SetRefreshToken(authResult);
                await SetLoggedInState(authResult);
                return true;
            }
            else
            {
                Crashes.TrackError(new Exception($"{result.Error}, {result.ErrorDescription}"));

                var signInResult = await SignInAsync();
                if (signInResult != ApiStatus.Success)
                {
                    Crashes.TrackError(new Exception(
                        $"Unsuccessful attempt to sign in after unsuccessful token refresh, ApiStatus={signInResult}"));
                }

                return signInResult == ApiStatus.Success;
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
            };
        }
        else if (result is RefreshTokenResult refreshTokenResult)
        {
            return new AuthResult
            {
                AccessToken = refreshTokenResult.AccessToken,
                RefreshToken = refreshTokenResult.RefreshToken,
                AccessTokenExpiration = refreshTokenResult.AccessTokenExpiration,
                IdentityToken = refreshTokenResult.IdentityToken,
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
    }
}

public class DetailsUpdatedEventArgs : EventArgs
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string Jobtitle { get; set; }
}
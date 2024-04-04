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
    bool HasCachedAccount { get; }
    event EventHandler DetailsUpdated;
}

public class AuthenticationService : IAuthenticationService
{
    private readonly OidcClientOptions _options;

    private string RefreshToken;

    private string _accessToken;
    private DateTimeOffset _tokenExpiry;

    public event EventHandler DetailsUpdated;
    public bool HasCachedAccount { get => Preferences.Get(nameof(HasCachedAccount), false); }

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
                Crashes.TrackError(new Exception($"AuthDebug: LoginAsync returned error {result.ErrorDescription}"));
                SignOut();
                return ApiStatus.Error;
            }

            var authResult = GetAuthResult(result);
            await SetRefreshToken(authResult);
            return SetLoggedInState(authResult);
        }
        catch (TaskCanceledException) // Is thrown when user closes the browser without logging-in
        {
            return ApiStatus.CancelledByUser;
        }
        catch (Exception ex)
        {
            Crashes.TrackError(new Exception($"AuthDebug: unknown exception was thrown during SignIn ${ex.Message}; ${ex.StackTrace}"));
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

    private ApiStatus SetLoggedInState(AuthResult loginResult)
    {
        if (!string.IsNullOrWhiteSpace(loginResult.IdentityToken) && !string.IsNullOrWhiteSpace(loginResult.AccessToken))
        {
            _accessToken = loginResult.AccessToken;
            _tokenExpiry = loginResult.AccessTokenExpiration;

            try
            {
                Preferences.Set(nameof(HasCachedAccount), true);
                DetailsUpdated?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                Crashes.TrackError(new Exception("Failed to set a logged-in state"));
                return ApiStatus.Unavailable;
            }

            return ApiStatus.Success;
        }
        else
        {
            Crashes.TrackError(new Exception(
                $"AuthDebug: loginResult is missing tokens. Missing IdentityToken = {string.IsNullOrWhiteSpace(loginResult.IdentityToken)}, missing AccessToken = {string.IsNullOrWhiteSpace(loginResult.AccessToken)}"));
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
                SetLoggedInState(authResult);
                return true;
            }
            else
            {
                Crashes.TrackError(new Exception($"{result.Error}, {result.ErrorDescription}"));

                var signInResult = await SignInAsync();
                if (signInResult != ApiStatus.Success && signInResult != ApiStatus.CancelledByUser)
                {
                    Crashes.TrackError(new Exception(
                        $"AuthDebug: Unsuccessful attempt to sign in after unsuccessful token refresh, ApiStatus={signInResult}"));
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
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Logging;
using SSW.Rewards.Mobile.Services.Authentication;

namespace SSW.Rewards.Mobile.Services;

public interface IAuthenticationService
{
    Task AutologinAsync(string accessToken);
    Task<ApiStatus> SignInAsync();
    Task<string> GetAccessToken();
    Task SignOut();
    bool HasCachedAccount { get; }
    bool IsLoggedIn { get; }
    event EventHandler DetailsUpdated;
    void NavigateToLoginPage();
}

public class AuthenticationService : IAuthenticationService
{
    private readonly ITokenManager _tokenManager;
    private readonly IOidcAuthenticationProvider _oidcProvider;
    private readonly IAuthStorageService _authStorage;
    private readonly INavigationService _navigationService;
    private readonly ILogger<AuthenticationService> _logger;

    public event EventHandler DetailsUpdated;
    public bool HasCachedAccount => _authStorage.HasCachedAccount;
    public bool IsLoggedIn => _tokenManager.IsLoggedIn;

    public AuthenticationService(
        ITokenManager tokenManager,
        IOidcAuthenticationProvider oidcProvider,
        IAuthStorageService authStorage,
        INavigationService navigationService,
        ILogger<AuthenticationService> logger)
    {
        _tokenManager = tokenManager;
        _oidcProvider = oidcProvider;
        _authStorage = authStorage;
        _navigationService = navigationService;
        _logger = logger;

        // Forward token updates to authentication service listeners
        _tokenManager.TokensUpdated += (sender, args) => DetailsUpdated?.Invoke(this, EventArgs.Empty);
    }

    public async Task AutologinAsync(string accessToken)
    {
        try
        {
            _logger.LogInformation("Attempting auto-login with access token");

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(accessToken);
            var expiry = jwtToken.ValidTo;

            await _tokenManager.StoreTokensAsync(accessToken, null, expiry);

            try
            {
                await _navigationService.InitializeMainPageAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize main page during auto-login");

                // TECH DEBT: Workaround for iOS since calling DisplayAlert while a Safari web view is in
                // the process of closing causes the alert to never appear and the await call never returns.
                if (DeviceInfo.Platform == DevicePlatform.iOS)
                {
                    await Task.Delay(1000);
                }

                await App.Current.MainPage.DisplayAlert("Login Failure", "There seems to have been a problem logging you in. Please try again.", "OK");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Auto-login failed");
            await SignOut();
        }
    }

    public async Task<ApiStatus> SignInAsync()
    {
        try
        {
            _logger.LogInformation("Starting sign-in process");

            var result = await _oidcProvider.LoginAsync(!HasCachedAccount);

            if (result.IsSuccess)
            {
                await _tokenManager.StoreTokensAsync(
                    result.AccessToken, 
                    result.RefreshToken, 
                    result.AccessTokenExpiration);

                _logger.LogInformation("Sign-in successful");
                return ApiStatus.Success;
            }

            _logger.LogWarning("Sign-in failed: {Error}", result.Error);
            await SignOut();
            return ApiStatus.Error;
        }
        catch (TaskCanceledException)
        {
            _logger.LogInformation("Sign-in cancelled by user");
            return ApiStatus.CancelledByUser;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during sign-in");
            await SignOut();
            return ApiStatus.Error;
        }
    }

    public async Task<string> GetAccessToken()
    {
        return await _tokenManager.GetValidTokenAsync();
    }

    public async Task SignOut()
    {
        _logger.LogInformation("Signing out user");
        await _tokenManager.ClearTokensAsync();
    }

    public void NavigateToLoginPage()
    {
        _navigationService.NavigateToLoginPage();
    }
}
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SSW.Rewards.Mobile.Services.Authentication;

public interface ITokenManager
{
    Task<string> GetValidTokenAsync();
    Task StoreTokensAsync(string accessToken, string refreshToken, DateTimeOffset expiry);
    Task ClearTokensAsync();
    bool IsLoggedIn { get; }
    event EventHandler TokensUpdated;
}

public class TokenManager : ITokenManager
{
    private readonly IAuthStorageService _storage;
    private readonly IOidcAuthenticationProvider _oidcProvider;
    private readonly ILogger<TokenManager> _logger;
    private readonly SemaphoreSlim _refreshSemaphore = new(1, 1);
    private readonly TimeSpan _refreshSemaphoreTimeout = TimeSpan.FromSeconds(10);
    private readonly TimeSpan _tokenRefreshBuffer;

    private string _cachedAccessToken;
    private DateTimeOffset _tokenExpiry;

    public bool IsLoggedIn => !string.IsNullOrWhiteSpace(_cachedAccessToken);
    public event EventHandler TokensUpdated;

    public TokenManager(
        IAuthStorageService storage,
        IOidcAuthenticationProvider oidcProvider,
        IOptions<AuthenticationOptions> options,
        ILogger<TokenManager> logger)
    {
        _storage = storage;
        _oidcProvider = oidcProvider;
        _logger = logger;
        _tokenRefreshBuffer = options.Value.TokenRefreshBuffer;
    }

    public async Task<string> GetValidTokenAsync()
    {
        await EnsureTokenLoadedAsync();

        if (IsCachedTokenValid())
        {
            return _cachedAccessToken;
        }

        return await HandleExpiredTokenAsync();
    }

    private async Task EnsureTokenLoadedAsync()
    {
        if (string.IsNullOrEmpty(_cachedAccessToken))
        {
            await LoadTokenFromStorageAsync();
        }
    }

    private async Task<string> HandleExpiredTokenAsync()
    {
        // Return cached token and refresh in background if available
        if (!string.IsNullOrEmpty(_cachedAccessToken))
        {
            _ = RefreshTokenAsync();
            _logger.LogInformation("Returning cached token for immediate use, refresh in progress");
            return _cachedAccessToken;
        }

        // No cached token - attempt refresh
        var success = await RefreshTokenAsync();

        if (success)
        {
            return _cachedAccessToken;
        }

        _logger.LogWarning("Unable to refresh token - no cached credentials available");
        return string.Empty;
    }

    private async Task<bool> RefreshTokenAsync()
    {
        if (!await _refreshSemaphore.WaitAsync(_refreshSemaphoreTimeout))
        {
            _logger.LogWarning("Token refresh timed out waiting for semaphore");
            return false;
        }

        try
        {
            // Double-check if token is now valid after acquiring semaphore
            if (IsCachedTokenValid())
            {
                return true;
            }

            var refreshToken = await _storage.GetRefreshTokenAsync();

            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                _logger.LogWarning("No refresh token available");
                return false;
            }

            var result = await _oidcProvider.RefreshTokenAsync(refreshToken);

            if (result.IsSuccess)
            {
                await StoreTokensAsync(result.AccessToken, result.RefreshToken, result.AccessTokenExpiration);
                return true;
            }

            _logger.LogWarning("Token refresh failed: {Error}", result.Error);
            return await AttemptSilentLoginAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Token refresh failed");
            return false;
        }
        finally
        {
            _refreshSemaphore.Release();
        }
    }

    public async Task StoreTokensAsync(string accessToken, string refreshToken, DateTimeOffset expiry)
    {
        _cachedAccessToken = accessToken;
        _tokenExpiry = expiry;

        if (!string.IsNullOrWhiteSpace(accessToken))
        {
            await _storage.StoreAccessTokenAsync(accessToken);
        }

        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            await _storage.StoreRefreshTokenAsync(refreshToken);
        }

        _storage.SetHasCachedAccount(true);
        TokensUpdated?.Invoke(this, EventArgs.Empty);
    }

    public async Task ClearTokensAsync()
    {
        _cachedAccessToken = null;
        _tokenExpiry = DateTimeOffset.MinValue;
        await _storage.ClearAllAsync();
    }

    private async Task LoadTokenFromStorageAsync()
    {
        var token = await _storage.GetAccessTokenAsync();

        if (string.IsNullOrEmpty(token))
        {
            return;
        }

        try
        {
            var expiry = ExtractTokenExpiry(token);

            if (!IsTokenValid(token))
            {
                _logger.LogInformation("Stored token is expired");
                _cachedAccessToken = null;
                _storage.ClearAccessToken();
            }
            else
            {
                _storage.SetHasCachedAccount(true);
                _cachedAccessToken = token;
                _tokenExpiry = expiry;
                TokensUpdated?.Invoke(this, EventArgs.Empty);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing stored access token");
            _cachedAccessToken = null;
            await _storage.ClearAllAsync();
        }
    }

    private static DateTimeOffset ExtractTokenExpiry(string accessToken)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(accessToken);
        return jwtToken.ValidTo;
    }

    private bool IsCachedTokenValid()
    {
        return IsTokenValid(_cachedAccessToken);
    }

    private bool IsTokenValid(string token)
    {
        return !string.IsNullOrWhiteSpace(token) &&
               _tokenExpiry > DateTimeOffset.UtcNow.Add(_tokenRefreshBuffer);
    }

    private async Task<bool> AttemptSilentLoginAsync()
    {
        var result = await _oidcProvider.SilentLoginAsync();

        if (result.IsSuccess)
        {
            await StoreTokensAsync(result.AccessToken, result.RefreshToken, result.AccessTokenExpiration);
            return true;
        }

        _logger.LogWarning("Silent login failed: {Error}", result.Error);
        return false;
    }
}
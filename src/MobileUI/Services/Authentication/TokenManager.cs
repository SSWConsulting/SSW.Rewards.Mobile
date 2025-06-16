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
        // Load from cache if not present
        if (string.IsNullOrEmpty(_cachedAccessToken))
        {
            await LoadTokenFromStorageAsync();
        }

        // Return if still valid (with buffer)
        if (IsTokenValid())
        {
            return _cachedAccessToken;
        }

        // Thread-safe token refresh with timeout
        bool lockTaken = await _refreshSemaphore.WaitAsync(TimeSpan.FromSeconds(30));
        if (!lockTaken)
        {
            _logger.LogWarning("Token refresh timed out waiting for semaphore");
            return string.Empty;
        }

        try
        {
            // Double-check after acquiring lock
            if (IsTokenValid())
            {
                return _cachedAccessToken;
            }

            if (await RefreshTokenAsync())
            {
                return _cachedAccessToken;
            }

            _logger.LogWarning("Unable to refresh token - clearing stored tokens");
            await ClearTokensAsync(); // Clear invalid tokens
            return string.Empty;
        }
        finally
        {
            _refreshSemaphore.Release();
        }
    }

    private async Task<bool> RefreshTokenAsync()
    {
        string refreshToken = await _storage.GetRefreshTokenAsync();

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
        else
        {
            _logger.LogWarning("Token refresh failed: {Error}", result.Error);

            // Try silent login as fallback
            return await AttemptSilentLoginAsync();
        }
    }

    public async Task StoreTokensAsync(string accessToken, string refreshToken, DateTimeOffset expiry)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            throw new ArgumentException("Access token cannot be null or empty", nameof(accessToken));
        }

        _cachedAccessToken = accessToken;
        _tokenExpiry = expiry;

        await _storage.StoreAccessTokenAsync(accessToken);

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
        _cachedAccessToken = await _storage.GetAccessTokenAsync();

        if (!string.IsNullOrEmpty(_cachedAccessToken))
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(_cachedAccessToken);
                _tokenExpiry = jwtToken.ValidTo;

                if (!IsTokenValid())
                {
                    _logger.LogInformation("Stored token is expired");
                    _cachedAccessToken = null;
                }
                else
                {
                    _storage.SetHasCachedAccount(true);
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
    }

    private bool IsTokenValid()
    {
        return !string.IsNullOrWhiteSpace(_cachedAccessToken) && 
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
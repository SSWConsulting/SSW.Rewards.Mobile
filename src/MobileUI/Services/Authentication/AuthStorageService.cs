using Microsoft.Extensions.Logging;

namespace SSW.Rewards.Mobile.Services.Authentication;

public interface IAuthStorageService
{
    Task<string> GetAccessTokenAsync();
    Task<string> GetRefreshTokenAsync();
    Task<string> GetDeviceTokenAsync();
    Task StoreAccessTokenAsync(string accessToken);
    Task StoreRefreshTokenAsync(string refreshToken);
    Task StoreDeviceTokenAsync(string deviceToken);
    Task ClearAllAsync();
    void SetHasCachedAccount(bool value);
    bool HasCachedAccount { get; }
    void SetIsFirstRun(bool value);
    bool IsFirstRun { get; }
    void SetDeviceTokenLastUpdated(DateTime value);
    DateTime DeviceTokenLastUpdated { get; }
}

public class AuthStorageService : IAuthStorageService
{
    private const string AccessTokenKey = "AccessToken";
    private const string RefreshTokenKey = "RefreshToken";
    private const string DeviceTokenKey = "DeviceToken";
    private const string HasCachedAccountKey = "HasCachedAccount";
    private const string FirstRunKey = "FirstRun";
    private const string DeviceTokenLastUpdatedKey = "DeviceTokenLastTimeUpdated";

    private readonly ILogger<AuthStorageService> _logger;

    public AuthStorageService(ILogger<AuthStorageService> logger)
    {
        _logger = logger;
    }

    public bool HasCachedAccount => Preferences.Get(HasCachedAccountKey, false);
    public bool IsFirstRun => Preferences.Get(FirstRunKey, true);
    public DateTime DeviceTokenLastUpdated => Preferences.Get(DeviceTokenLastUpdatedKey, DateTime.MinValue);

    public async Task<string> GetAccessTokenAsync()
    {
        return await GetTokenFromSecureStorageAsync(AccessTokenKey);
    }

    public async Task<string> GetRefreshTokenAsync()
    {
        return await GetTokenFromSecureStorageAsync(RefreshTokenKey);
    }

    public async Task<string> GetDeviceTokenAsync()
    {
        return await GetTokenFromSecureStorageAsync(DeviceTokenKey);
    }

    public async Task StoreAccessTokenAsync(string accessToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(accessToken, nameof(accessToken));
        await StoreTokenInSecureStorageAsync(AccessTokenKey, accessToken);
    }

    public async Task StoreRefreshTokenAsync(string refreshToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(refreshToken, nameof(refreshToken));
        await StoreTokenInSecureStorageAsync(RefreshTokenKey, refreshToken);
    }

    public async Task StoreDeviceTokenAsync(string deviceToken)
    {
        if (string.IsNullOrWhiteSpace(deviceToken))
        {
            _logger.LogWarning("Attempted to store null or empty device token");
            return;
        }

        await StoreTokenInSecureStorageAsync(DeviceTokenKey, deviceToken);
    }

    public async Task ClearAllAsync()
    {
        try
        {
            var deviceToken = await GetDeviceTokenAsync();

            SecureStorage.RemoveAll();
            Preferences.Clear();

            // Restore device token and first run state
            if (!string.IsNullOrWhiteSpace(deviceToken))
            {
                await StoreDeviceTokenAsync(deviceToken);
            }

            // Preserve first run state
            SetIsFirstRun(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clear storage");
            throw;
        }
    }

    public void SetHasCachedAccount(bool value)
    {
        SetPreference(HasCachedAccountKey, value);
    }

    public void SetIsFirstRun(bool value)
    {
        SetPreference(FirstRunKey, value);
    }

    public void SetDeviceTokenLastUpdated(DateTime value)
    {
        SetPreference(DeviceTokenLastUpdatedKey, value);
    }

    private async Task<string> GetTokenFromSecureStorageAsync(string key)
    {
        try
        {
            return await SecureStorage.GetAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve '{Key}' from secure storage", key);
            return null;
        }
    }

    private async Task StoreTokenInSecureStorageAsync(string key, string token)
    {
        try
        {
            await SecureStorage.SetAsync(key, token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to store '{Key}' in secure storage", key);
            throw;
        }
    }

    private void SetPreference(string key, bool value)
    {
        try
        {
            Preferences.Set(key, value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set '{Key}' preference", key);
            throw;
        }
    }

    private void SetPreference(string key, DateTime value)
    {
        try
        {
            Preferences.Set(key, value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set '{Key}' preference", key);
            throw;
        }
    }
}
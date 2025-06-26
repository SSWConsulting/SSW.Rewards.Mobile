using Microsoft.Extensions.Logging;
using Mopups.Services;
using SSW.Rewards.Mobile.Common;
using SSW.Rewards.Mobile.Services.Authentication;

namespace SSW.Rewards.Mobile.Services;

public interface IFirstRunService
{
    Task InitialiseAfterLogin();
    void SetPendingScanCode(string code);
}

public class FirstRunService : IFirstRunService
{
    private readonly IServiceProvider _provider;
    private readonly IPermissionsService _permissionsService;
    private readonly IPushNotificationsService _pushNotificationsService;
    private readonly IAuthStorageService _storage;
    private readonly IUserService _userService;
    private readonly ILogger<FirstRunService> _logger;

    private string _pendingScanCode;

    public FirstRunService(IServiceProvider provider, IPermissionsService permissionsService,
        IPushNotificationsService pushNotificationsService, IAuthStorageService storage,
        IUserService userService, ILogger<FirstRunService> logger)
    {
        _provider = provider;
        _permissionsService = permissionsService;
        _pushNotificationsService = pushNotificationsService;
        _storage = storage;
        _userService = userService;
        _logger = logger;
    }

    public async Task InitialiseAfterLogin()
    {
        Application.Current.InitializeMainPage();

        // Update user details after login
        try
        {
            await _userService.UpdateMyDetailsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user details after login");
        }

        var granted = await _permissionsService.CheckAndRequestPermission<Permissions.PostNotifications>();

        Task uploadDeviceTokenTask = null;
        if (granted)
        {
            uploadDeviceTokenTask = UploadDeviceTokenIfRequired();
        }

        if (_storage.IsFirstRun)
        {
            _storage.SetIsFirstRun(false);
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                var page = ActivatorUtilities.CreateInstance<OnBoardingPage>(_provider);
                await MopupService.Instance.PushAsync(page);
            });
        }
        else if (!string.IsNullOrEmpty(_pendingScanCode))
        {
            var vm = ActivatorUtilities.CreateInstance<ScanResultViewModel>(_provider);
            var popup = new PopupPages.ScanResult(vm, _pendingScanCode);
            await MopupService.Instance.PushAsync(popup);
            _pendingScanCode = null;
        }

        if (uploadDeviceTokenTask != null)
        {
            try
            {
                await uploadDeviceTokenTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading device token");
            }
        }
    }
    
    public void SetPendingScanCode(string code)
    {
        _pendingScanCode = code;
    }

    private async Task UploadDeviceTokenIfRequired()
    {
        var now = DateTime.UtcNow;
        var lastTimeUpdated = _storage.DeviceTokenLastUpdated;
        if (now <= lastTimeUpdated.AddDays(30))
        {
            return;
        }

        var token = await _storage.GetDeviceTokenAsync();
        if (string.IsNullOrWhiteSpace(token))
        {
            return;
        }

        var success = await _pushNotificationsService.UploadDeviceToken(token, now, DeviceService.GetDeviceId());
        if (success)
        {
            _storage.SetDeviceTokenLastUpdated(now);
        }
    }
}

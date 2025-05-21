using Mopups.Services;
using SSW.Rewards.Mobile.Common;

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
    
    private string _pendingScanCode;

    public FirstRunService(IServiceProvider provider, IPermissionsService permissionsService, IPushNotificationsService pushNotificationsService)
    {
        _provider = provider;
        _permissionsService = permissionsService;
        _pushNotificationsService = pushNotificationsService;
    }

    public async Task InitialiseAfterLogin()
    {
        Application.Current.InitializeMainPage();

        var granted = await _permissionsService.CheckAndRequestPermission<Permissions.PostNotifications>();
        if (granted)
        {
            await UploadDeviceTokenIfRequired();
        }

        if (Preferences.Get("FirstRun", true))
        {
            Preferences.Set("FirstRun", false);
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
    }
    
    public void SetPendingScanCode(string code)
    {
        _pendingScanCode = code;
    }

    private async Task UploadDeviceTokenIfRequired()
    {
        var now = DateTime.Now;
        var lastTimeUpdated = Preferences.Get("DeviceTokenLastTimeUpdated", DateTime.MinValue);
        if (now <= lastTimeUpdated.AddDays(30))
        {
            return;
        }

        var token = await SecureStorage.GetAsync("DeviceToken");
        if (string.IsNullOrWhiteSpace(token))
        {
            return;
        }

        var success = await _pushNotificationsService.UploadDeviceToken(token, now, DeviceService.GetDeviceId());
        if (success)
        {
            Preferences.Set("DeviceTokenLastTimeUpdated", now);
        }
    }
}

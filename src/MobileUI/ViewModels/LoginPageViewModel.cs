using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AppCenter.Crashes;

namespace SSW.Rewards.Mobile.ViewModels;

public partial class LoginPageViewModel : BaseViewModel
{
    private readonly IAuthenticationService _authService;
    private readonly IPushNotificationsService _pushNotificationsService;

    [ObservableProperty]
    private bool _isRunning;

    [ObservableProperty]
    private bool _loginButtonEnabled;

    [ObservableProperty]
    private string _buttonText;

    bool _isStaff;

    public LoginPageViewModel(IAuthenticationService authService, IUserService userService, IPushNotificationsService pushNotificationsService)
    {
        _authService = authService;
        _pushNotificationsService = pushNotificationsService;
        ButtonText = "Sign up / Log in";
        userService.MyQrCodeObservable().Subscribe(myQrCode => _isStaff = !string.IsNullOrWhiteSpace(myQrCode));
    }

    [RelayCommand]
    private async Task LoginTapped()
    {
        IsRunning = true;
        LoginButtonEnabled = false;
        bool enableButtonAfterLogin = true;

        ApiStatus status = await _authService.SignInAsync();

        var statusAlerts = new Dictionary<ApiStatus, (string Title, string Message)>
        {
            { ApiStatus.Unavailable, ("Service Unavailable", "Looks like the SSW.Rewards service is not currently available. Please try again later.") },
            { ApiStatus.LoginFailure, ("Login Failure", "There seems to have been a problem logging you in. Please try again.") },
        };

        if (status != ApiStatus.CancelledByUser)
        {
            if (status != ApiStatus.Success)
            {
                await WaitForWindowClose();
                var alert = statusAlerts.GetValueOrDefault(status, (Title: "Unexpected Error", Message: "Something went wrong there, please try again later."));
                await App.Current.MainPage.DisplayAlert(alert.Title, alert.Message, "OK");
            }
            else
            {
                enableButtonAfterLogin = false;
                await OnAfterLogin();
            }
        }

        LoginButtonEnabled = enableButtonAfterLogin;
        IsRunning = false;
    }

    private async static Task WaitForWindowClose()
    {
        // TECH DEBT: Workaround for iOS since calling DisplayAlert while a Safari web view is in
        // the process of closing causes the alert to never appear and the await call never returns.
        if (DeviceInfo.Platform == DevicePlatform.iOS)
        {
            await Task.Delay(1000);
        }
    }

    public async Task Refresh()
    {
        if (!_authService.HasCachedAccount)
        {
            LoginButtonEnabled = true;
            return;
        }

        bool enableButtonAfterLogin = true;
        LoginButtonEnabled = false;
        IsRunning = true;
        ButtonText = "Logging you in...";

        try
        {
            if (!string.IsNullOrEmpty(await _authService.GetAccessToken()))
            {
                enableButtonAfterLogin = false;

                await OnAfterLogin();
            }
        }
        catch (Exception e)
        {
            // Everything else is fatal
            Crashes.TrackError(e);
            Console.WriteLine(e);
            await WaitForWindowClose();
            await Application.Current.MainPage.DisplayAlert("Login Failure",
                "There seems to have been a problem logging you in. Please try again. " + e.Message, "OK");
        }
        finally
        {
            IsRunning = false;
            LoginButtonEnabled = enableButtonAfterLogin;
            ButtonText = "Sign up / Log in";
        }
    }

    private async Task OnAfterLogin()
    {
        Application.Current.MainPage = App.ResolveShell(_isStaff);
        await UploadDeviceTokenIfRequired();
        await Shell.Current.GoToAsync("//main");
    }

    /// <summary>
    /// Updates FCM device token on the server every 30 days as per Google's recommendation:
    /// https://firebase.google.com/docs/cloud-messaging/manage-tokens#update-tokens-on-a-regular-basis
    /// </summary>
    private async Task UploadDeviceTokenIfRequired()
    {
        var now = DateTime.Now;
        var lastTimeUpdated = Preferences.Get("DeviceTokenLastTimeUpdated", DateTime.MaxValue.AddDays(-30));
        if (lastTimeUpdated.AddDays(30) < now)
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

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Plugin.Firebase.Crashlytics;

namespace SSW.Rewards.Mobile.ViewModels;

public partial class LoginPageViewModel : BaseViewModel
{
    private readonly IAuthenticationService _authService;

    [ObservableProperty]
    private bool _isRunning;

    [ObservableProperty]
    private bool _loginButtonEnabled;

    [ObservableProperty]
    private string _buttonText;

    bool _isStaff;

    public LoginPageViewModel(
        IAuthenticationService authService,
        IUserService userService)
    {
        _authService = authService;
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

                // Only display error if user is not logged in.
                // Autologin will fall here, if login page is opened, despite being successfull.
                if (_authService.IsLoggedIn && _authService.HasCachedAccount)
                {
                    var alert = statusAlerts.GetValueOrDefault(status, (Title: "Unexpected Error", Message: "Something went wrong there, please try again later."));
                    await Shell.Current.DisplayAlert(alert.Title, alert.Message, "OK");
                }
            }
            else
            {
                enableButtonAfterLogin = false;
                await App.InitialiseMainPage();
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

                await App.InitialiseMainPage();
            }
        }
        catch (Exception e)
        {
            // Everything else is fatal
            CrossFirebaseCrashlytics.Current.RecordException(e);
            Console.WriteLine(e);
            await WaitForWindowClose();
            await Shell.Current.DisplayAlert("Login Failure",
                "There seems to have been a problem logging you in. Please try again. " + e.Message, "OK");
        }
        finally
        {
            IsRunning = false;
            LoginButtonEnabled = enableButtonAfterLogin;
            ButtonText = "Sign up / Log in";
        }
    }
}

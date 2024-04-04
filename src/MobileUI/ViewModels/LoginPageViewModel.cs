using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AppCenter.Crashes;

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

    public LoginPageViewModel(IAuthenticationService authService, IUserService userService)
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
        bool enableButtonAfterLogin = true;

        if (_authService.HasCachedAccount)
        {
            LoginButtonEnabled = false;
            IsRunning = true;
            ButtonText = "Logging you in...";

            try
            {
                if(!string.IsNullOrEmpty(await _authService.GetAccessToken()))
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
        else
        {
            LoginButtonEnabled = true;
        }
    }

    private async Task OnAfterLogin()
    {
        Application.Current.MainPage = App.ResolveShell(_isStaff);
        await Shell.Current.GoToAsync("//main");
    }
}

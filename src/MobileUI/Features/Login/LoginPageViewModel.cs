using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using SSW.Rewards.Mobile.Common;

namespace SSW.Rewards.Mobile.ViewModels;

public partial class LoginPageViewModel : BaseViewModel
{
    private readonly IAuthenticationService _authService;
    private readonly ILogger<LoginPageViewModel> _logger;

    [ObservableProperty]
    private bool _isRunning;

    [ObservableProperty]
    private bool _loginButtonEnabled;

    [ObservableProperty]
    private string _buttonText;

    private bool _isStaff;

    public LoginPageViewModel(IAuthenticationService authService, IUserService userService, ILogger<LoginPageViewModel> logger)
    {
        _authService = authService;
        _logger = logger;
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
                    await App.Current.MainPage.DisplayAlert(alert.Title, alert.Message, "OK");
                }
            }
            else
            {
                enableButtonAfterLogin = false;
                await App.InitialiseMainPageAsync();
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
            // Load token in background.
            var _ = _authService.GetAccessToken();

            await WaitForWindowClose();
            await App.InitialiseMainPageAsync();
        }
        catch (HttpRequestException e)
        {
            // Everything else is fatal
            _logger.LogError(e, "HTTP request exception on login refresh");
            await WaitForWindowClose();

            // Skip logic for initial setup as the above might have failed on updating device ID.
            await Application.Current.InitializeMainPage();
        }
        catch (Exception e)
        {
            // Everything else is fatal
            _logger.LogError(e, "Error refreshing token");
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
}

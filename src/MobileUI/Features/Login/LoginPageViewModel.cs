using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

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

    public LoginPageViewModel(IAuthenticationService authService, IUserService userService, ILogger<LoginPageViewModel> logger)
    {
        _authService = authService;
        _logger = logger;
        ButtonText = "Sign up / Log in";
    }

    [RelayCommand]
    private async Task LoginTapped()
    {
        SetLoadingState(true);

        try
        {
            ApiStatus status = await _authService.SignInAsync();

            if (status == ApiStatus.Success)
            {
                await App.InitialiseMainPageAsync();
                return;
            }

            if (status != ApiStatus.CancelledByUser)
            {
                await WaitForWindowClose();
                await ShowErrorForStatus(status);
            }
        }
        finally
        {
            SetLoadingState(false);
        }
    }

    private void SetLoadingState(bool isLoading)
    {
        IsRunning = isLoading;
        LoginButtonEnabled = !isLoading;
        ButtonText = isLoading ? "Logging you in..." : "Sign up / Log in";
    }

    private async Task ShowErrorForStatus(ApiStatus status)
    {
        var statusAlerts = new Dictionary<ApiStatus, (string Title, string Message)>
        {
            { ApiStatus.Unavailable, ("Service Unavailable", "The SSW.Rewards service is not currently available. Please try again later.") },
            { ApiStatus.LoginFailure, ("Login Failure", "There was a problem logging you in. Please try again.") },
            { ApiStatus.Error, ("Unexpected Error", "Something went wrong. Please try again later.") }
        };

        var alert = statusAlerts.GetValueOrDefault(status, ("Error", "An unexpected error occurred."));
        await App.Current.MainPage.DisplayAlert(alert.Item1, alert.Item2, "OK");
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

        SetLoadingState(true);

        try
        {
            var token = await _authService.GetAccessTokenAsync();
        
            if (!string.IsNullOrEmpty(token))
            {
                await WaitForWindowClose();
                await App.InitialiseMainPageAsync();
                return;
            }
        
            // Token retrieval failed - show session expired message
            await WaitForWindowClose();
            await Application.Current.MainPage.DisplayAlert("Session Expired", 
                "Your session has expired. Please log in again.", "OK");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error during refresh");
            await WaitForWindowClose();
            await Application.Current.MainPage.DisplayAlert("Login Error",
                "There was a problem refreshing your session. Please try logging in again.", "OK");
        }
        finally
        {
            SetLoadingState(false);
        }
    }
}
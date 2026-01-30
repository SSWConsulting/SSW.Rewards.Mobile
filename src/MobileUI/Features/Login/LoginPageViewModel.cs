using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace SSW.Rewards.Mobile.ViewModels;

public partial class LoginPageViewModel : BaseViewModel
{
    private readonly IAuthenticationService _authService;
    private readonly ILogger<LoginPageViewModel> _logger;
    private readonly IAlertService _alertService;

    private const string DefaultButtonText = "Sign up / Log in";

    [ObservableProperty]
    private bool _isRunning;

    [ObservableProperty]
    private bool _loginButtonEnabled;

    [ObservableProperty]
    private string _buttonText;

    public LoginPageViewModel(IAuthenticationService authService, ILogger<LoginPageViewModel> logger, IAlertService alertService)
    {
        _authService = authService;
        _logger = logger;
        _alertService = alertService;
        ButtonText = DefaultButtonText;
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
                await ShowErrorForStatus(status, _alertService);
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
        ButtonText = isLoading ? "Logging you in..." : DefaultButtonText;
    }

    private static async Task ShowErrorForStatus(ApiStatus status, IAlertService alertService)
    {
        var statusAlerts = new Dictionary<ApiStatus, (string Title, string Message)>
        {
            { ApiStatus.Unavailable, ("Service Unavailable", "The SSW.Rewards service is not currently available. Please try again later.") },
            { ApiStatus.LoginFailure, ("Login Failure", "There was a problem logging you in. Please try again.") },
            { ApiStatus.Error, ("Unexpected Error", "Something went wrong. Please try again later.") }
        };

        var alert = statusAlerts.GetValueOrDefault(status, ("Error", "An unexpected error occurred."));
        await alertService.ShowAlertAsync(alert.Item1, alert.Item2, "OK");
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
            // Proceed to main page immediately if we have a cached account
            await WaitForWindowClose();
            await App.InitialiseMainPageAsync();

            try
            {
                var token = await _authService.GetAccessTokenAsync();

                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("No access token found");
                    return;
                }

                _logger.LogInformation("Access token retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get access token - user can continue with cached credentials");
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error during refresh");
            await WaitForWindowClose();
            await _alertService.ShowAlertAsync("Login Error",
                "There was a problem accessing your account. Please try logging in again.", "OK");
        }
        finally
        {
            SetLoadingState(false);
        }
    }
}
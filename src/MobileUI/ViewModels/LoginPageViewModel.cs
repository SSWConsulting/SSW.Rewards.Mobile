using CommunityToolkit.Mvvm.Input;
using Microsoft.AppCenter.Crashes;

namespace SSW.Rewards.Mobile.ViewModels;

public partial class LoginPageViewModel : BaseViewModel
{
    [ObservableProperty]
    private bool _isRunning;

    [ObservableProperty]
    private bool _loginButtonEnabled;

    bool _isStaff = false;

    private readonly IUserService _userService;
    private readonly IAuthenticationService _authService;

    public string ButtonText { get; set; }

    public LoginPageViewModel(IAuthenticationService authService, IUserService userService)
    {
        _authService = authService;
        _userService = userService;
        ButtonText = "Sign up / Log in";
        OnPropertyChanged("ButtonText");
    }

    [RelayCommand]
    private async Task LoginTapped()
    {
        IsRunning = true;
        LoginButtonEnabled = false;
        bool enablebuttonAfterLogin = true;

        ApiStatus status;
        try
        {
            status = await _authService.SignInAsync();
        }
        catch (Exception exception)
        {
            status = ApiStatus.LoginFailure;
            //Crashes.TrackError(exception);
            await App.Current.MainPage.DisplayAlert("Login Failure", exception.Message, "OK");
        }

        switch (status)
        {
            case ApiStatus.Success:
                enablebuttonAfterLogin = false;
                await OnAfterLogin();
                break;
            case ApiStatus.Unavailable:
                await App.Current.MainPage.DisplayAlert("Service Unavailable", "Looks like the SSW.Rewards service is not currently available. Please try again later.", "OK");
                break;
            case ApiStatus.LoginFailure:
                await App.Current.MainPage.DisplayAlert("Login Failure", "There seems to have been a problem logging you in. Please try again.", "OK");
                break;
            default:
                await App.Current.MainPage.DisplayAlert("Unexpected Error", "Something went wrong there, please try again later.", "OK");
                break;
        }

        LoginButtonEnabled = enablebuttonAfterLogin;
        IsRunning = false;
    }

    public async Task Refresh()
    {
        bool enablebuttonAfterLogin = true;

        if (_userService.HasCachedAccount)
        {
            LoginButtonEnabled = false;
            IsRunning = true;
            ButtonText = "Logging you in...";

            try
            {
                if(!string.IsNullOrEmpty(await _authService.GetAccessToken()))
                {
                    enablebuttonAfterLogin = false;

                    await OnAfterLogin();

                }
            }
            catch (Exception e)
            {
                // Everything else is fatal
                Crashes.TrackError(e);
                Console.WriteLine(e);
                await Application.Current.MainPage.DisplayAlert("Login Failure",
                    "There seems to have been a problem logging you in. Please try again. " + e.Message, "OK");
            }
            finally
            {
                IsRunning = false;
                LoginButtonEnabled = enablebuttonAfterLogin;
                ButtonText = "Sign up / Log in";
            }
        }
    }

    private async Task OnAfterLogin()
    {
        var qr = _userService.MyQrCode;
        if (!string.IsNullOrWhiteSpace(qr))
        {
            _isStaff = true;
        }
        else
        {
            _isStaff = false;
        }

        Application.Current.MainPage = App.ResolveShell(_isStaff);
        await Shell.Current.GoToAsync("//main");
    }
}

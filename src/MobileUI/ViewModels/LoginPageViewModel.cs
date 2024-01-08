using CommunityToolkit.Mvvm.Input;
using Microsoft.AppCenter.Crashes;

namespace SSW.Rewards.Mobile.ViewModels;

public partial class LoginPageViewModel : BaseViewModel
{

    public bool isRunning { get; set; }

    public bool LoginButtonEnabled { get; set; }

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
        isRunning = true;
        LoginButtonEnabled = false;
        bool enablebuttonAfterLogin = true;
        RaisePropertyChanged(nameof(isRunning), nameof(LoginButtonEnabled));

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
        isRunning = false;
        RaisePropertyChanged(nameof(isRunning), nameof(LoginButtonEnabled));
    }

    public async Task Refresh()
    {
        bool enablebuttonAfterLogin = true;

        if (_userService.HasCachedAccount)
        {
            LoginButtonEnabled = false;
            isRunning = true;
            ButtonText = "Logging you in...";
            RaisePropertyChanged(nameof(isRunning), nameof(ButtonText), nameof(LoginButtonEnabled));

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
                isRunning = false;
                LoginButtonEnabled = enablebuttonAfterLogin;
                ButtonText = "Sign up / Log in";
                RaisePropertyChanged(nameof(ButtonText), nameof(isRunning), nameof(LoginButtonEnabled));
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

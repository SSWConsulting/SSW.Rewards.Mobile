using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.AppCenter.Crashes;

namespace SSW.Rewards.Mobile.ViewModels;

public partial class LoginPageViewModel : BaseViewModel
{
    public ICommand LoginTappedCommand { get; set; }

    private IUserService _userService { get; set; }

    [ObservableProperty]
    private bool _isRunning;

    [ObservableProperty]
    private bool _loginButtonEnabled;

    bool _isStaff = false;

    [ObservableProperty]
    private string _buttonText;

    public LoginPageViewModel(IUserService userService)
    {
        _userService = userService;
        LoginTappedCommand = new Command(SignIn);
        ButtonText = "Sign up / Log in";
    }

    private async void SignIn()
    {
        IsRunning = true;
        LoginButtonEnabled = false;
        bool enablebuttonAfterLogin = true;

        ApiStatus status;
        try
        {
            status = await _userService.SignInAsync();
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
                if(await _userService.RefreshLoginAsync())
                {
                    // TODO: Do we need this in a refresh?
                    await _userService.UpdateMyDetailsAsync();

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

    async Task OnForgotPassword()
    {
        await _userService.ResetPassword();
    }
}

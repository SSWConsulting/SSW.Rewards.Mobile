using System.Windows.Input;

namespace SSW.Rewards.ViewModels;

public class LoginPageViewModel : BaseViewModel
{
    public ICommand LoginTappedCommand { get; set; }
    
    private IUserService _userService { get; set; }
    
    public bool isRunning { get; set; }

    public bool LoginButtonEnabled { get; set; }

    bool _isStaff = false;

    public string ButtonText { get; set; }

    public LoginPageViewModel(IUserService userService)
    {
        _userService = userService;
        LoginTappedCommand = new Command(SignIn);
        LoginButtonEnabled = true;
        ButtonText = "Sign up / Log in";
        OnPropertyChanged("ButtonText");
    }

    private async void SignIn()
    {
        isRunning = true;
        LoginButtonEnabled = false;
        bool enablebuttonAfterLogin = true;
        RaisePropertyChanged(nameof(isRunning), nameof(LoginButtonEnabled));

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
                await Navigation.PopModalAsync();
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
                if(await _userService.RefreshLoginAsync())
                {
                    // TODO: Do we need this in a refresh?
                    await _userService.UpdateMyDetailsAsync();

                    enablebuttonAfterLogin = false;

                    await Navigation.PopModalAsync();

                }
            }
            catch (Exception e)
            {
                // Everything else is fatal
                //Crashes.TrackError(e);
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

    async Task OnForgotPassword()
    {
        await _userService.ResetPassword();
    }
}
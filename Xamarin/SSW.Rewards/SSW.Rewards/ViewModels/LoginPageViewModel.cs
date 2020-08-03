using System;
using System.Windows.Input;
using SSW.Rewards.Services;
using Xamarin.Forms;
using Xamarin.Essentials;
using SSW.Rewards.Models;
using Microsoft.AppCenter.Crashes;

namespace SSW.Rewards.ViewModels
{
    public class LoginPageViewModel : BaseViewModel
    {
        public ICommand LoginTappedCommand { get; set; }
        private IUserService _userService { get; set; }
        public bool isRunning { get; set; }
        public bool LoginButtonEnabled { get { return !isRunning; } }

        public string ButtonText { get; set; }

        public LoginPageViewModel(IUserService userService)
        {
            _userService = userService;
            LoginTappedCommand = new Command(SignIn);
            ButtonText = "Sign up / Log in";
            OnPropertyChanged("ButtonText");
            Refresh();
        }

        private async void SignIn()
        {
            isRunning = true;
            RaisePropertyChanged(new string[] { "isRunning", "LoginButtonEnabled" });

            ApiStatus status;
            try
            {
                status = await _userService.SignInAsync();
            }
            catch (Exception exception)
            {
                status = ApiStatus.LoginFailure;
                Crashes.TrackError(exception);
            }

            switch (status)
            {
                case ApiStatus.Success:
                    Application.Current.MainPage = Resolver.Resolve<AppShell>();
                    await Shell.Current.GoToAsync("//main");
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

            isRunning = false;
            RaisePropertyChanged(new string[] { "isRunning", "LoginButtonEnabled" });
        }

        private async void Refresh()
        {
            bool loggedIn = await _userService.IsLoggedInAsync();

            if (loggedIn)
            {
                isRunning = true;
                ButtonText = "Logging you in...";
                RaisePropertyChanged("isRunning", "ButtonText", "LoginButtonEnabled");

                UserInformation userInfo = await Auth.SignInAsync();
                string token = userInfo.AccessToken;
                await SecureStorage.SetAsync("auth_token", token);

                await _userService.UpdateMyDetailsAsync();

                Application.Current.MainPage = Resolver.Resolve<AppShell>();
                await Shell.Current.GoToAsync("//main");
            }
        }
    }
}

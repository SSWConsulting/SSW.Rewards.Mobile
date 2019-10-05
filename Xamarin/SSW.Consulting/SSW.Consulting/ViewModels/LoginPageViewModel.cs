using System;
using System.Windows.Input;
using SSW.Consulting.Services;
using Xamarin.Forms;
using Xamarin.Essentials;

namespace SSW.Consulting.ViewModels
{
    public class LoginPageViewModel : BaseViewModel
    {
        public ICommand LoginTappedCommand { get; set; }
        private IUserService _userService { get; set; }
        public bool isRunning { get; set; }

        public LoginPageViewModel(IUserService userService)
        {
            _userService = userService;
            LoginTappedCommand = new Command(SignIn);
        }

        private async void SignIn()
        {
            isRunning = true;
            OnPropertyChanged("isRunning");

            if(await _userService.SignInAsync())
            {
                Application.Current.MainPage = Resolver.Resolve<AppShell>();
                await Shell.Current.GoToAsync("//main");
            }

            isRunning = false;
            OnPropertyChanged("isRunning");
        }
    }
}

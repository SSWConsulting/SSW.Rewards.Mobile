using System;
using System.Windows.Input;
using Microsoft.AppCenter.Auth;
using SSW.Consulting.Services;
using Xamarin.Forms;
using Xamarin.Essentials;

namespace SSW.Consulting.ViewModels
{
    public class LoginPageViewModel : BaseViewModel
    {
        public ICommand LoginTappedCommand { get; set; }
        private IUserService _userService { get; set; }

        public LoginPageViewModel(IUserService userService)
        {
            _userService = userService;
            LoginTappedCommand = new Command(SignIn);
        }

        private async void SignIn()
        {
            UserInformation userInfo = await Auth.SignInAsync();
            await _userService.SignInAsync(userInfo);
        }
    }
}

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
            try
            {
                // Sign-in succeeded.
                UserInformation userInfo = await Auth.SignInAsync();
                string accountId = userInfo.AccountId;
                //Application.Current.MainPage.DisplayAlert("Message", accountId, "OK");
                if(!string.IsNullOrWhiteSpace(accountId))
                {
                    await _userService.SetTokenAsync(accountId);
                    Preferences.Set("LoggedIn", true);
                }
                else
                {
                    //TODO: handle login error
                }
            }

            catch (Exception e)
            {
                // Do something with sign-in failure.
            }
        }
    }
}

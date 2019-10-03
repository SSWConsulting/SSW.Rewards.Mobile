using System;
using System.Windows.Input;
using SSW.Consulting.Services;

namespace SSW.Consulting.ViewModels
{
    public class LoginPageViewModel : BaseViewModel
    {
        public ICommand LoginTappedCommand { get; set; }
        private IUserService _userService { get; set; }

        public LoginPageViewModel(IUserService userService)
        {
            _userService = userService;
        }
    }
}

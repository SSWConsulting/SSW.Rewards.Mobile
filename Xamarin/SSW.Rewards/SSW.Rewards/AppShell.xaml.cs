using System;
using System.Linq;
using Rg.Plugins.Popup.Services;
using SSW.Rewards.PopupPages;
using SSW.Rewards.Services;
using SSW.Rewards.Views;
using Xamarin.Forms;

namespace SSW.Rewards
{
    public partial class AppShell : Xamarin.Forms.Shell
    {
        private IUserService _userService { get; set; }

        public async void Handle_LogOutClicked(object sender, EventArgs e)
        {
            _userService.SignOut();
            await Navigation.PushModalAsync(new LoginPage());
        }

        public async void Handle_QuizClicked(object sender, EventArgs e)
        {
            await PopupNavigation.Instance.PushAsync(new TechQuiz());
        }

        public async void Handle_EventsClicked(object sender, EventArgs e)
        {
            await PopupNavigation.Instance.PushAsync(new Events());
        }

        public async void Handle_JoinClicked(object sender, EventArgs e)
        {
            await PopupNavigation.Instance.PushAsync(new JoinUs());
        }

        public async void Handle_AboutClicked(object sender, EventArgs e)
        {
            await PopupNavigation.Instance.PushAsync(new AboutSSW());
        }

        public void Handle_HowToPlayClicked(object sender, EventArgs e)
        {
            Navigation.PushModalAsync(new OnBoarding());
        }

        private async void Handle_QRClicked(object sender, EventArgs e)
        {
            var qrCode = await _userService.GetMyQrCode();

            await PopupNavigation.Instance.PushAsync(new MyQRPage(qrCode));
        }

        protected override bool OnBackButtonPressed()
		{
			if (Application.Current.MainPage.GetType() == typeof(AppShell) && Shell.Current.Navigation.NavigationStack.Where(x => x != null).Any())
			{
				return base.OnBackButtonPressed();
			}
			else
			{
				System.Diagnostics.Process.GetCurrentProcess().CloseMainWindow();
				return true;
			}
		}

		public AppShell(IUserService userService)
        {
            InitializeComponent();
            _userService = userService;
            _userService.UserLoggedIn += OnUserLoggedIn;
        }

        private void OnUserLoggedIn(object sender, UserLoggedInEventArgs e)
        {
            if (e.IsStaff)
            {
                QRMenuItem.IsEnabled = true;
            }
            else
            {
                QRMenuItem.IsEnabled = false;
            }
        }
    }
}

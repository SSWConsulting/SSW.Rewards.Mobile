using Rg.Plugins.Popup.Services;
using SSW.Rewards.PopupPages;
using SSW.Rewards.Services;
using SSW.Rewards.Views;
using System;
using System.Linq;
using Xamarin.Forms;

namespace SSW.Rewards
{
    public partial class AppShell : Xamarin.Forms.Shell
    {
        private IUserService _userService { get; set; }

        bool _showQRMenu;

        public AppShell(IUserService userService, bool isStaff)
        {
            InitializeComponent();
            _userService = userService;
            ShowQRCodeMenuItem = isStaff;
            BindingContext = this;
        }

        public bool ShowQRCodeMenuItem
        {
            get => _showQRMenu;
            set
            {
                _showQRMenu = value;
                OnPropertyChanged();
            }
        }

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

            if (!string.IsNullOrWhiteSpace(qrCode))
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
    }
}

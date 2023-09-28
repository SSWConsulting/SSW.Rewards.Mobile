using Mopups.Pages;
using Mopups.Services;

namespace SSW.Rewards.PopupPages
{
    public partial class TechQuiz : PopupPage
    {
        private IUserService _userService { get; set;
        }
        public TechQuiz(IUserService userService)
        {
            InitializeComponent();
            _userService = userService;
        }

        private async void FindoutMore_Tapped(object sender, EventArgs e)
        {
            string quizUri = Constants.ApiBaseUrl + "/api/achievement/techquiz?user=" + _userService.MyEmail;

            await Browser.OpenAsync(quizUri, BrowserLaunchMode.External);
        }

        public async void Handle_CloseTapped(object sender, EventArgs args)
        {
            //            DisplayAlert("Close Tapped", "Close", "OK");
            await MopupService.Instance.PopAllAsync();
        }
    }
}

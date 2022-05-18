using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Rg.Plugins.Popup.Services;
using SSW.Rewards.Models;
using SSW.Rewards.PopupPages;
using SSW.Rewards.Services;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace SSW.Rewards.ViewModels
{
    public class EarnPointsViewModel : BaseViewModel
    {
        private IChallengeService _challengeService { get; set; }
        private IUserService _userService { get; set; }

        public ObservableCollection<ExternalReward> ExternalRewards { get; set; }
        public ICommand OnScanTapped { get; set; }
        public ICommand OnEventTapped { get; set; }

        private string _twitterAchievement = "U1NXL1NTVyBUViBUd2l0dGVy";
        private string _sswTVAchievement = "U1NXIFRW";

        public EarnPointsViewModel(IChallengeService challengeService, IUserService userService)
        {
            Title = "Earn Points";
            _challengeService = challengeService;
            _userService = userService;
            ExternalRewards = new ObservableCollection<ExternalReward>();
            OnScanTapped = new Command(OpenQRScanner);
			OnEventTapped = new Command<string>((x) => OpenURL(x));
            Initialise();
        }

        private void Initialise()
        {
            string quizUri = App.Constants.ApiBaseUrl + "/api/achievement/techquiz?user=" + _userService.MyEmail;

            ExternalRewards = new ObservableCollection<ExternalReward>
            {
                new ExternalReward { Badge = "link", IsBonus = true, Points = 100, Title="Follow us on Twitter", Picture = "points_twitter", Url = "https://twitter.com/SSW_TV"},
                new ExternalReward { Badge = "link", IsBonus = true, Points = 500, Title="Take SSW's Tech Quiz", Picture = "points_quiz", Url = quizUri },
                new ExternalReward { Badge = "external", IsBonus = true, Points = 100, Title="Subscribe to SSW TV", Picture = "points_youtube", Url = "https://www.youtube.com/channel/UCBFgwtV9lIIhvoNh0xoQ7Pg"},
                new ExternalReward { Badge = "link", IsBonus = false, Points = 0, Title="See SSW's events", Picture = "points_presentations", Url = "https://www.ssw.com.au/ssw/Events/?upcomingeventsonly=true"},
                new ExternalReward { Badge = "link", IsBonus = false, Points = 0, Title="Check out SSW Software Audits", Picture = "points_audit", Url = "https://www.ssw.com.au/ssw/Consulting/Software-Audit.aspx"}
            };
        }

        private async void OpenURL(string url)
        {
            if(url.Contains("twitter"))
                await _challengeService.ValidateQRCodeAsync(_twitterAchievement);
             
            else if(url.Contains("youtube"))
                await _challengeService.ValidateQRCodeAsync(_sswTVAchievement);

            await Browser.OpenAsync(url, BrowserLaunchMode.External);
            MessagingCenter.Send<object>(this, "NewAchievement");
        }

        private async void OpenQRScanner()
        {
            await PopupNavigation.Instance.PushAsync(new QRScannerPage());
        }
    }
}

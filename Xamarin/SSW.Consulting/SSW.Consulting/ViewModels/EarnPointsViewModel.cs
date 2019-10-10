using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Rg.Plugins.Popup.Services;
using SSW.Consulting.Models;
using SSW.Consulting.PopupPages;
using SSW.Consulting.Services;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace SSW.Consulting.ViewModels
{
    public class EarnPointsViewModel : BaseViewModel
    {
        private IChallengeService _challengeService { get; set; }
        public ObservableCollection<ExternalReward> ExternalRewards { get; set; }
        public ICommand OnScanTapped { get; set; }
        public ICommand OnEventTapped { get; set; }

        public EarnPointsViewModel(IChallengeService challengeService)
        {
            Title = "Earn Points";
            _challengeService = challengeService;
            ExternalRewards = new ObservableCollection<ExternalReward>();
            OnScanTapped = new Command(OpenQRScanner);
			OnEventTapped = new Command<string>((x) => OpenURL(x));
			Initialise();
        }

        private async void Initialise()
        {
            ExternalRewards = new ObservableCollection<ExternalReward>
            {
                new ExternalReward { Badge = "link", IsBonus = false, Points = 10, Title="Follow us on Twitter", Picture = "points_twitter", Url = "https://twitter.com/SSW_TV"},
                new ExternalReward { Badge = "link", IsBonus = false, Points = 10, Title="Take SSW tech quiz", Picture = "points_quiz", Url = Constants.ApiBaseUrl + "/api/achievement/techquiz" },
                new ExternalReward { Badge = "external", IsBonus = false, Points = 10, Title="Subscribe to SSW TV", Picture = "points_youtube", Url = "https://www.youtube.com/channel/UCBFgwtV9lIIhvoNh0xoQ7Pg"},
                new ExternalReward { Badge = "link", IsBonus = false, Points = 10, Title="See SSW events", Picture = "points_presentations", Url = "https://www.ssw.com.au/ssw/Events/?upcomingeventsonly=true"}
            };
        }

        private async void OpenURL(string url)
        {
            await Browser.OpenAsync(url, BrowserLaunchMode.External);
        }

        private async void OpenQRScanner()
        {
            await PopupNavigation.Instance.PushAsync(new QRScannerPage());
        }
    }
}

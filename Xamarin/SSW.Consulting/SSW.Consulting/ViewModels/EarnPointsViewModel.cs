using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Rg.Plugins.Popup.Services;
using SSW.Consulting.Models;
using SSW.Consulting.PopupPages;
using SSW.Consulting.Services;
using Xamarin.Forms;

namespace SSW.Consulting.ViewModels
{
    public class EarnPointsViewModel : BaseViewModel
    {
        private IChallengeService _challengeService { get; set; }
        public ObservableCollection<Challenge> Challenges { get; set; }
        public ICommand OnScanTapped { get; set; }

        public EarnPointsViewModel(IChallengeService challengeService)
        {
            Title = "Earn Points";
            _challengeService = challengeService;
            Challenges = new ObservableCollection<Challenge>();
            OnScanTapped = new Command(OpenQRScanner);
            Initialise();
        }

        private async void Initialise()
        {
            var challenges = await _challengeService.GetChallengesAsync();
            foreach(var challenge in challenges)
            {
                Challenges.Add(challenge);
            }
        }

        private async void OpenQRScanner()
        {
            await PopupNavigation.Instance.PushAsync(new QRScannerPage());
        }
    }
}

using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Rg.Plugins.Popup.Services;
using SSW.Consulting.Models;
using SSW.Consulting.PopupPages;
using SSW.Consulting.Services;
using Xamarin.Forms;
using ZXing;

namespace SSW.Consulting.ViewModels
{
    public class QRScannerPageViewModel : BaseViewModel
    {
        public Result Result { get; set; }
        private IChallengeService _challengeService { get; set; }

        public QRScannerPageViewModel(IChallengeService challengeService)
        {
            _challengeService = challengeService;
        }

        public async Task CheckAchievement(Result result)
        {
            ChallengeResultViewModel challenge = await _challengeService.PostChallengeAsync(result.Text);
            await PopupNavigation.Instance.PushAsync(new ScanResult(challenge));
        }
    }
}

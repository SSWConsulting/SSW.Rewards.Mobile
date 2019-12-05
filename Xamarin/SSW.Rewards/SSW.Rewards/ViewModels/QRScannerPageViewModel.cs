using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Rg.Plugins.Popup.Services;
using SSW.Rewards.Models;
using SSW.Rewards.PopupPages;
using SSW.Rewards.Services;
using Xamarin.Forms;
using ZXing;

namespace SSW.Rewards.ViewModels
{
    public class QRScannerPageViewModel : BaseViewModel
    {
        public async Task CheckAchievement(Result result)
        {
            await PopupNavigation.Instance.PushAsync(new ScanResult(result.Text));
        }
    }
}

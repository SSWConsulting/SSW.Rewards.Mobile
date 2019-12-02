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
        public async Task CheckAchievement(Result result)
        {
            await PopupNavigation.Instance.PushAsync(new ScanResult(result.Text));
        }
    }
}

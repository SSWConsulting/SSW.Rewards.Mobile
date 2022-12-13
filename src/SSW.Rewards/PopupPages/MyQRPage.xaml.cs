using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms.Xaml;

namespace SSW.Rewards.PopupPages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MyQRPage : PopupPage
    {
        public MyQRPage(string qrCode)
        {
            InitializeComponent();

            codeView.BarcodeValue = qrCode;
        }

        private async void Close_Clicked(object sender, System.EventArgs e)
        {
            await PopupNavigation.Instance.PopAllAsync();
        }
    }
}
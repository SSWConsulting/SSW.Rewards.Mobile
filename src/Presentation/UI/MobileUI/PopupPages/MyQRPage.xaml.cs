using Mopups.Pages;
using Mopups.Services;

namespace SSW.Rewards.PopupPages;

public partial class MyQRPage : PopupPage
{
    public MyQRPage(string qrCode)
    {
        InitializeComponent();

        codeView.Value = qrCode;
    }

    private async void Close_Clicked(object sender, System.EventArgs e)
    {
        await MopupService.Instance.PopAllAsync();
    }
}
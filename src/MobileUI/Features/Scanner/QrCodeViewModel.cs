using Mopups.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SSW.Rewards.Mobile.ViewModels;

public partial class QrCodeViewModel : BaseViewModel
{
    [ObservableProperty]
    private string _qrCode;
    
    public QrCodeViewModel(IUserService userService)
    {
        userService.MyQrCodeObservable().Subscribe(myQrCode => QrCode = myQrCode);
    }
    
    [RelayCommand]
    private static async Task ClosePopup()
    {
        await MopupService.Instance.PopAsync();
    }
}
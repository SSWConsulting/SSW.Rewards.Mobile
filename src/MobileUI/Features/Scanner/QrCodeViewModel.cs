using Mopups.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QRCoder;

namespace SSW.Rewards.Mobile.ViewModels;

public partial class QrCodeViewModel : BaseViewModel
{
    [ObservableProperty]
    private ImageSource _qrCode;
    
    public QrCodeViewModel(IUserService userService)
    {
        userService.MyQrCodeObservable().Subscribe(GenerateQrCode);
    }
    
    private void GenerateQrCode(string qrCodeString)
    {
        using QRCodeGenerator qrGenerator = new();
        using QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrCodeString, QRCodeGenerator.ECCLevel.Q);
        using PngByteQRCode qrCode = new(qrCodeData);
        
        byte[] qrCodeBytes = qrCode.GetGraphic(20);
        QrCode = ImageSource.FromStream(() => new MemoryStream(qrCodeBytes));
    }
    
    [RelayCommand]
    private static async Task ClosePopup()
    {
        await MopupService.Instance.PopAsync();
    }
}
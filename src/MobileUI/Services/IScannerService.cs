namespace SSW.Rewards.Mobile.Services;

public interface IScannerService
{
    Task<ScanResponseViewModel> ValidateQRCodeAsync(string achievementString);
}

using SSW.Rewards.ViewModels;

namespace SSW.Rewards.Services;

public interface IScannerService
{
    Task<ScanResponseViewModel> ValidateQRCodeAsync(string achievementString);
}

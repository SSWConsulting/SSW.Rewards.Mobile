using SSW.Rewards.ViewModels;
using System.Threading.Tasks;

namespace SSW.Rewards.Services
{
    public interface IScannerService
    {
        Task<ScanResponseViewModel> ValidateQRCodeAsync(string achievementString);
    }
}

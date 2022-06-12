using SSW.Rewards.Models;
using SSW.Rewards.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SSW.Rewards.Services
{
    public interface IScannerService
    {
        Task<ScanResponseViewModel> ValidateQRCodeAsync(string achievementString);
    }
}

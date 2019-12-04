using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SSW.Rewards.Models;
using SSW.Rewards.ViewModels;

namespace SSW.Rewards.Services
{
    public interface IChallengeService
    {
        Task<IEnumerable<Challenge>> GetChallengesAsync();
        Task<ChallengeResultViewModel> ValidateQRCodeAsync(string achievementString);
    }
}

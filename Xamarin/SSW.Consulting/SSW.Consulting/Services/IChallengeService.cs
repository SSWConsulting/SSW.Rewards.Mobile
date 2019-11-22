using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SSW.Consulting.Models;
using SSW.Consulting.ViewModels;

namespace SSW.Consulting.Services
{
    public interface IChallengeService
    {
        Task<IEnumerable<Challenge>> GetChallengesAsync();
        Task<IEnumerable<MyChallenge>> GetMyChallengesAsync();
        Task<ChallengeResultViewModel> ValidateQRCodeAsync(string achievementString);
    }
}

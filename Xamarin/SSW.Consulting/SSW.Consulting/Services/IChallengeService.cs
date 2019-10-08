using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SSW.Consulting.Models;

namespace SSW.Consulting.Services
{
    public interface IChallengeService
    {
        Task<IEnumerable<Challenge>> GetChallengesAsync();
        Task<IEnumerable<MyChallenge>> GetMyChallengesAsync();
        Task<ChallengeResult> PostChallengeAsync(string achievementString);
    }
}

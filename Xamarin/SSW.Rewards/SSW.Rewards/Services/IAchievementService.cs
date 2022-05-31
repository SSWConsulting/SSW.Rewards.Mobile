using System.Threading.Tasks;

namespace SSW.Rewards.Services
{
    public interface IAchievementService
    {
        Task<PostAchievementResult> PostAchievementAsync(string qrCode);
    }
}

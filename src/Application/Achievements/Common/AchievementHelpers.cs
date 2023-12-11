using System.Text;

namespace SSW.Rewards.Application.Achievements.Common;
public static class AchievementHelpers
{
    public static string GenerateCode(string inputValue, bool IsReward)
    {
        var prefix = IsReward ? "rwd:" : "ach:";
        var codeData = Encoding.ASCII.GetBytes($"{prefix}{inputValue}");
        return Convert.ToBase64String(codeData);
    }
}

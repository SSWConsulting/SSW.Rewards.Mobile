using System.Text;

namespace SSW.Rewards.Application.Common.Helpers;
public static class AchievementHelper
{
    public static string GenerateCode(string inputValue)
    {
        var codeData = Encoding.ASCII.GetBytes($"ach:{inputValue}");
        return Convert.ToBase64String(codeData);
    }
}

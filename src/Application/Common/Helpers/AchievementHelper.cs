using System.Text;

namespace SSW.Rewards.Application.Common.Helpers;
public static class AchievementHelper
{
    public static string GenerateCode()
    {
        var codeData = Encoding.ASCII.GetBytes($"ach:{Guid.NewGuid().ToString()}");
        return Convert.ToBase64String(codeData);
    }
}

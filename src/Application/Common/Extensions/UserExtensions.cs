using System.Net.Mail;
using System.Text;
using SSW.Rewards.Shared.DTOs.Users;

namespace SSW.Rewards.Application.Common.Extensions;

public static class UserExtensions
{
    public static bool IsStaff(this CurrentUserDto viewModel)
    {
        var emailAddress = new MailAddress(viewModel.Email);

        if (emailAddress.Host == "ssw.com.au")
        {
            return true;
        }

        return false;
    }

    public static void GenerateAchievement(this User user)
    {
        var codeData = Encoding.ASCII.GetBytes($"ach:{Guid.NewGuid().ToString()}");
        var code = Convert.ToBase64String(codeData);

        var achievement = new Achievement
        {
            Name = user.FullName,
            Value = 100,
            Type = AchievementType.Scanned,
            IsMultiscanEnabled = false,
            Code = code
        };

        user.Achievement = achievement;
    }
}

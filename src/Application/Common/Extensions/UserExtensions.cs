using System.Net.Mail;
using SSW.Rewards.Application.Common.Helpers;
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
        var achievement = new Achievement
        {
            Name = user.FullName,
            Value = 100,
            Type = AchievementType.Scanned,
            IsMultiscanEnabled = false,
            Code = AchievementHelper.GenerateCode(Guid.NewGuid().ToString()),
        };

        user.Achievement = achievement;
    }
}

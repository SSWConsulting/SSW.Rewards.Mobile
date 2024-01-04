using System.Net.Mail;
using Shared.DTOs.Users;

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
}

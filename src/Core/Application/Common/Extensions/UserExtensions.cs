using System.Net.Mail;
using SSW.Rewards.Application.Users.Queries.GetCurrentUser;

namespace SSW.Rewards.Application.Common.Extensions;

public static class UserExtensions
{
    public static bool IsStaff(this CurrentUserViewModel viewModel)
    {
        var emailAddress = new MailAddress(viewModel.Email);

        if (emailAddress.Host == "ssw.com.au")
        {
            return true;
        }

        return false;
    }
}

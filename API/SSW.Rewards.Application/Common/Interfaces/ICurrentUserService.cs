using SSW.Rewards.Application.Users.Queries.GetCurrentUser;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.Rewards.Application.Common.Interfaces
{
    public interface ICurrentUserService
    {
        string GetUserId();
        string GetUserEmail();
        string GetUserFullName();
        string GetUserProfilePic();

        Task<CurrentUserViewModel> GetCurrentUserAsync(CancellationToken cancellationToken);
    }
}

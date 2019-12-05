using SSW.Rewards.Application.User.Queries.GetCurrentUser;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.Rewards.Application.Common.Interfaces
{
    public interface ICurrentUserService
    {
        string GetUserId();
        string GetUserEmail();
        string GetUserFullName();
        string GetUserAvatar();

        Task<CurrentUserViewModel> GetCurrentUserAsync(CancellationToken cancellationToken);
    }
}

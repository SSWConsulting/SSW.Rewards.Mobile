using SSW.Consulting.Application.User.Queries.GetCurrentUser;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.Consulting.Application.Common.Interfaces
{
    public interface ICurrentUserService
    {
        string GetUserId();
        string GetUserEmail();
        string GetUserFullName();
        string GetUserAvatar();

        Task<CurrentUserViewModel> GetCurrentUser(CancellationToken cancellationToken);
    }
}

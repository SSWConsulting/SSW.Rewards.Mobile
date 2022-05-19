using MediatR;
using SSW.Rewards.Application.Common.Interfaces;
using SSW.Rewards.Application.Users.Common.Interfaces;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.Rewards.Application.Users.Queries.GetCurrentUserRoles
{
    public class GetCurrentUserRolesQuery : IRequest<string[]>
    {

    }

    public class GetCurrentUserRolesQuerHandler : IRequestHandler<GetCurrentUserRolesQuery, string[]>
    {
        private readonly IUserService _userService;

        public GetCurrentUserRolesQuerHandler(IUserService userService, ICurrentUserService currentUserService)
        {
            _userService = userService;
        }

        public async Task<string[]> Handle(GetCurrentUserRolesQuery request, CancellationToken cancellationToken)
        {
            var roles = await _userService.GetCurrentUserRoles(cancellationToken);

            return roles.Select(r => r.Name).ToArray();
        }
    }
}

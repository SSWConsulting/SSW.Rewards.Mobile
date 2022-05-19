using MediatR;
using SSW.Rewards.Application.Users.Common.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.Rewards.Application.Users.Queries.GetUserRewards
{
    public class GetUserRewardsQuery : IRequest<UserRewardsViewModel>
    {
        public int UserId { get; set; }
    }

    public class GetUserRewardsQueryHandler : IRequestHandler<GetUserRewardsQuery, UserRewardsViewModel>
    {
        private readonly IUserService _userService;

        public GetUserRewardsQueryHandler(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<UserRewardsViewModel> Handle(GetUserRewardsQuery request, CancellationToken cancellationToken)
        {
            return await _userService.GetUserRewards(request.UserId, cancellationToken);
        }
    }
}
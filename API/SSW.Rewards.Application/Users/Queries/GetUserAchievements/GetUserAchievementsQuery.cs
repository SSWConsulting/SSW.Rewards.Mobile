using AutoMapper;
using MediatR;
using SSW.Rewards.Application.Common.Interfaces;
using SSW.Rewards.Application.Users.Common.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.Rewards.Application.Users.Queries.GetUserAchievements
{
    public class GetUserAchievementsQuery : IRequest<UserAchievementsViewModel>
    {
        public int UserId { get; set; }        
    }

    public class GetUserAchievementsQueryHandler : IRequestHandler<GetUserAchievementsQuery, UserAchievementsViewModel>
    {
        private readonly IMapper _mapper;
        private readonly ISSWRewardsDbContext _context;
        private readonly IUserService _userService;

        public GetUserAchievementsQueryHandler(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<UserAchievementsViewModel> Handle(GetUserAchievementsQuery request, CancellationToken cancellationToken)
        {
            return await _userService.GetUserAchievements(request.UserId, cancellationToken);
        }
    }
}

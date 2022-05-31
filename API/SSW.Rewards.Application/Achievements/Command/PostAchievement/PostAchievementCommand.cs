using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SSW.Rewards.Application.Achievements.Queries.Common;
using SSW.Rewards.Application.Common.Interfaces;
using SSW.Rewards.Application.Users.Common.Interfaces;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.Rewards.Application.Achievements.Command.PostAchievement
{
    public class PostAchievementCommand : IRequest<PostAchievementResult>
    {
        public string Code { get; set; }

        public class PostAchievementCommandHandler : IRequestHandler<PostAchievementCommand, PostAchievementResult>
        {
            private readonly IUserService _userService;
            private readonly ISSWRewardsDbContext _context;
            private readonly IMapper _mapper;

            public PostAchievementCommandHandler(
                IUserService UserService,
                ISSWRewardsDbContext context,
                IMapper mapper)
            {
                _userService = UserService;
                _context = context;
                _mapper = mapper;
            }

            public async Task<PostAchievementResult> Handle(PostAchievementCommand request, CancellationToken cancellationToken)
            {
                var achievement = await _context
                   .Achievements
                   .Where(a => a.Code == request.Code)
                   .FirstOrDefaultAsync(cancellationToken);

                if (achievement == null)
                {
                    return new PostAchievementResult
                    {
                        status = AchievementStatus.NotFound
                    };
                }

                var user = await _userService.GetCurrentUser(cancellationToken);
                var userHasAchievement = await _context
                    .UserAchievements
                    .Where(ua => ua.UserId == user.Id)
                    .Where(ua => ua.AchievementId == achievement.Id)
                    .AnyAsync(cancellationToken);

                if (userHasAchievement)
                {
                    return new PostAchievementResult
                    {
                        status = AchievementStatus.Duplicate
                    };
                }

                await _context
                    .UserAchievements
                    .AddAsync(new Domain.Entities.UserAchievement
                    {
                        UserId = user.Id,
                        AchievementId = achievement.Id
                    }, cancellationToken);

                await _context.SaveChangesAsync(cancellationToken);

                var achievementModel = _mapper.Map<AchievementDto>(achievement);

                return new PostAchievementResult
                {
                    viewModel = achievementModel,
                    status = AchievementStatus.Added
                };
            }
        }
    }
}

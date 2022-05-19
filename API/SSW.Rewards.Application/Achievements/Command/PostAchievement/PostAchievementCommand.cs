using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SSW.Rewards.Application.Achievements.Queries.GetAchievementList;
using SSW.Rewards.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.Rewards.Application.Achievements.Command.PostAchievement
{
    public class PostAchievementCommand : IRequest<PostAchievementResult>
    {
        public string Code { get; set; }

        public class PostAchievementCommandHandler : IRequestHandler<PostAchievementCommand, PostAchievementResult>
        {
            private readonly ICurrentUserService _currentUserService;
            private readonly ISSWRewardsDbContext _context;
            private readonly IMapper _mapper;

            public PostAchievementCommandHandler(
                ICurrentUserService currentUserService,
                ISSWRewardsDbContext context,
                IMapper mapper)
            {
                _currentUserService = currentUserService;
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

                var user = await _currentUserService.GetCurrentUserAsync(cancellationToken);
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

                var achievementModel = _mapper.Map<AchievementViewModel>(achievement);

                return new PostAchievementResult
                {
                    viewModel = achievementModel,
                    status = AchievementStatus.Added
                };
            }
        }
    }
}

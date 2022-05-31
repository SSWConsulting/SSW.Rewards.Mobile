using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SSW.Rewards.Application.Achievements.Queries.Common;
using SSW.Rewards.Application.Common.Exceptions;
using SSW.Rewards.Application.Common.Interfaces;
using SSW.Rewards.Application.Users.Commands.UpsertUser;
using SSW.Rewards.Application.Users.Common.Interfaces;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.Rewards.Application.Achievements.Commands.AddAchievement
{
    public class AddAchievementCommand : IRequest<AchievementDto>
    {
        public string Code { get; set; }
    }

    public class AddAchievementCommandHandler : IRequestHandler<AddAchievementCommand, AchievementDto>
    {
        private readonly IUserService _userService;
        private readonly ISSWRewardsDbContext _context;
        private readonly IMapper _mapper;

        public AddAchievementCommandHandler(
            IUserService userService,
            ISSWRewardsDbContext context,
            IMapper mapper)
        {
            _userService = userService;
            _context = context;
            _mapper = mapper;
        }

        public async Task<AchievementDto> Handle(AddAchievementCommand request, CancellationToken cancellationToken)
        {
            var achievement = await _context
                .Achievements
                .Where(a => a.Code == request.Code)
                .FirstOrDefaultAsync(cancellationToken);

            if (achievement == null)
            {
                throw new NotFoundException(request.Code, nameof(Domain.Entities.Achievement));
            }

            var user = await _userService.GetCurrentUser(cancellationToken);

            var userHasAchievement = await _context
                .UserAchievements
                .Where(ua => ua.UserId == user.Id)
                .Where(ua => ua.AchievementId == achievement.Id)
                .AnyAsync(cancellationToken);

            if (userHasAchievement)
            {
                // TODO: check that the UI does this check before calling the API
                throw new AlreadyAwardedException(user.Id, achievement.Name);
            }

            await _context
                .UserAchievements
                .AddAsync(new Domain.Entities.UserAchievement
                {
                    UserId = user.Id,
                    AchievementId = achievement.Id
                }, cancellationToken);

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception)
            {
                // TODO: check that this is the right exception to throw here
                throw new AlreadyAwardedException(user.Id, achievement.Name);
            }

            return _mapper.Map<AchievementDto>(achievement);
        }
    }
}

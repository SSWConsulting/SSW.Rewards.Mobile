using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SSW.Consulting.Application.Achievement.Queries.GetAchievementList;
using SSW.Consulting.Application.Common.Interfaces;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SSW.Consulting.Application.Achievement.Command.AddAchievement;

namespace SSW.Consulting.Application.Achievement.Command.AddAchievement
{
    public class AddAchievementCommand : IRequest<AddAchievementResult>
    {
        public string Code { get; set; }

        public class AddAchievementCommandHandler : IRequestHandler<AddAchievementCommand, AddAchievementResult>
        {
            private readonly ICurrentUserService _currentUserService;
            private readonly ISSWConsultingDbContext _context;
            private readonly IMapper _mapper;

            public AddAchievementCommandHandler(
                ICurrentUserService currentUserService,
                ISSWConsultingDbContext context,
                IMapper mapper)
            {
                _currentUserService = currentUserService;
                _context = context;
                _mapper = mapper;
            }

            public async Task<AddAchievementResult> Handle(AddAchievementCommand request, CancellationToken cancellationToken)
            {
                var achievement = await _context
                    .Achievements
                    .Where(a => a.Code == request.Code)
                    .FirstOrDefaultAsync(cancellationToken);

                if (achievement == null)
                {
                    return new AddAchievementResult
                    {
                        Status = AchievementStatus.NotFound
                    };
                }

                var user = await _currentUserService.GetCurrentUser(cancellationToken);
                var userHasAchievement = await _context
                    .UserAchievements
                    .Where(ua => ua.UserId == user.Id)
                    .Where(ua => ua.AchievementId == achievement.Id)
                    .AnyAsync(cancellationToken);

                if (userHasAchievement)
                {
                    return new AddAchievementResult
                    {
                        Status = AchievementStatus.Duplicate
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

                return new AddAchievementResult
                {
                    ViewModel = achievementModel,
                    Status = AchievementStatus.Added
                };
            }
        }
    }
}

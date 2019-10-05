using MediatR;
using Microsoft.EntityFrameworkCore;
using SSW.Consulting.Application.Common.Exceptions;
using SSW.Consulting.Application.Common.Interfaces;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.Consulting.Application.User.Commands.UpsertUser
{
    public class AddAchievementCommand : IRequest<Unit>
    {
        public string Code { get; set; }

        public class AddAchievementCommandHandler : IRequestHandler<AddAchievementCommand, Unit>
        {
            private readonly ICurrentUserService _currentUserService;
            private readonly ISSWConsultingDbContext _context;

            public AddAchievementCommandHandler(
                ICurrentUserService currentUserService,
                ISSWConsultingDbContext context)
            {
                _currentUserService = currentUserService;
                _context = context;
            }

            public async Task<Unit> Handle(AddAchievementCommand request, CancellationToken cancellationToken)
            {
                var achievement = await _context
                    .Achievements
                    .Where(a => a.Code == request.Code)
                    .FirstOrDefaultAsync(cancellationToken);

                if (achievement == null)
                {
                    throw new NotFoundException(request.Code, nameof(Domain.Entities.Achievement));
                }

                var user = await _currentUserService.GetCurrentUser(cancellationToken);
                var userHasAchievement = await _context
                    .UserAchievements
                    .Where(ua => ua.UserId == user.Id)
                    .Where(ua => ua.AchievementId == achievement.Id)
                    .AnyAsync(cancellationToken);

                if (userHasAchievement)
                {
                    throw new AlreadyAwardedException(user.Id, achievement.Name);
                }

                await _context
                    .UserAchievements
                    .AddAsync(new Domain.Entities.UserAchievement
                    {
                        UserId = user.Id,
                        AchievementId = achievement.Id
                    });

                await _context.SaveChangesAsync(cancellationToken);


                return Unit.Value;
            }
        }
    }
}

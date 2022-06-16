using MediatR;
using Microsoft.EntityFrameworkCore;
using SSW.Rewards.Application.Common.Exceptions;
using SSW.Rewards.Application.Common.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.Rewards.Application.Achievements.Command.DeleteAchievement
{
    public class DeleteAchievementCommand : IRequest<Unit>
    {
        public int Id { get; set; }
    }

    public class DeleteAchievementCommandHandler : IRequestHandler<DeleteAchievementCommand, Unit>
    {
        private readonly ISSWRewardsDbContext _context;

        public DeleteAchievementCommandHandler(ISSWRewardsDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(DeleteAchievementCommand request, CancellationToken cancellationToken)
        {
            var achievement = await _context.Achievements
                        .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

            if (achievement == null)
            {
                throw new NotFoundException(nameof(DeleteAchievementCommand), request.Id);
            }

            achievement.IsDeleted = true;

            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}

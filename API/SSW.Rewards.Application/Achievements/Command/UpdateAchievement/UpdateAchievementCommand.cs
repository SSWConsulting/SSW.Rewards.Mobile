using MediatR;
using Microsoft.EntityFrameworkCore;
using SSW.Rewards.Application.Common.Exceptions;
using SSW.Rewards.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.Rewards.Application.Achievements.Command.UpdateAchievement
{
    public class UpdateAchievementCommand : IRequest<Unit>
    {
        public int Id { get; set; }
        public int Value { get; set; }

        public sealed class UpdateAchievementCommandHandler : IRequestHandler<UpdateAchievementCommand, Unit>
        {
            private readonly ISSWRewardsDbContext _context;

            public UpdateAchievementCommandHandler(ISSWRewardsDbContext context)
            {
                _context = context;
            }

            public async Task<Unit> Handle(UpdateAchievementCommand request, CancellationToken cancellationToken)
            {
                var achievement = await _context.Achievements
                                        .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

                if (achievement == null)
                {
                    throw new NotFoundException(nameof(UpdateAchievementCommandHandler), request.Id);
                }

                achievement.Value = request.Value;

                await _context.SaveChangesAsync(cancellationToken);
                return Unit.Value;
            }
        }
    }
}

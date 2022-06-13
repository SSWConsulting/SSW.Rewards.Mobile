using MediatR;
using Microsoft.EntityFrameworkCore;
using SSW.Rewards.Application.Common.Exceptions;
using SSW.Rewards.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;



namespace SSW.Rewards.Application.Rewards.Commands.UpdateReward
{
    public class UpdateRewardCommand : IRequest<Unit>
    {
        public int Id { get; set; }
        public int Cost { get; set; }

        public sealed class UpdateRewardCommandHandler : IRequestHandler<UpdateRewardCommand, Unit>
        {
            private readonly ISSWRewardsDbContext _context;

            public UpdateRewardCommandHandler(ISSWRewardsDbContext context)
            {
                _context = context;
            }

            public async Task<Unit> Handle(UpdateRewardCommand request, CancellationToken cancellationToken)
            {
                var reward = await _context.Rewards
                        .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

                if (reward == null)
                {
                    throw new NotFoundException(nameof(UpdateRewardCommand), request.Id);
                }

                reward.Cost = request.Cost;

                await _context.SaveChangesAsync(cancellationToken);
                return Unit.Value;
            }
        }
    }
}

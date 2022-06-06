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

namespace SSW.Rewards.Application.Rewards.Commands.DeleteReward
{
    public class DeleteRewardCommand : IRequest<Unit>
    {
        public int Id { get; set; }

        public class DeleteRewardCommandHandler : IRequestHandler<DeleteRewardCommand, Unit>
        {
            private readonly ISSWRewardsDbContext _context;

            public DeleteRewardCommandHandler(ISSWRewardsDbContext context)
            {
                _context = context;
            }

            public async Task<Unit> Handle(DeleteRewardCommand request, CancellationToken cancellationToken)
            {
                var findReward = await _context?.Rewards?
                    .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

                if (findReward == null)
                {
                    throw new NotFoundException(nameof(DeleteRewardCommand), request.Id);
                }

                _context.Rewards.Remove(findReward);
                await _context.SaveChangesAsync(cancellationToken);

                return Unit.Value;

            }
        }
    }
}

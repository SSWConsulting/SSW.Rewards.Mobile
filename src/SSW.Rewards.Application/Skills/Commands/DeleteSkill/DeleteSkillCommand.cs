using MediatR;
using SSW.Rewards.Application.Common.Exceptions;
using SSW.Rewards.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.Rewards.Application.Skills.Commands.DeleteSkill
{
    public class DeleteSkillCommand : IRequest<Unit>
    {
        public int Id{ get; set; }

        public sealed class UpsertSkillCommandHandler : IRequestHandler<DeleteSkillCommand, Unit>
        {
            private readonly ISSWRewardsDbContext _context;

            public UpsertSkillCommandHandler(ISSWRewardsDbContext context)
            {
                _context = context;
            }

            public async Task<Unit> Handle(DeleteSkillCommand request, CancellationToken cancellationToken)
            {
                if (request?.Id == null)
                {
                    throw new Exception("Bad Request");
                }

                var found = _context.Skills.FirstOrDefault(x => x.Id == request.Id);

                if (found == null)
                {
                    throw new NotFoundException("Bad Request");
                }

                _context.Skills.Remove(found);
                await _context.SaveChangesAsync(cancellationToken);
                return Unit.Value;
            }
        }
    }
}

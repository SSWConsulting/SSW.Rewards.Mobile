using MediatR;
using SSW.Rewards.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.Rewards.Application.Skills.Commands.UpsertSkill
{
    public class UpsertSkillCommand : IRequest<Unit>
    {
        public int Id { get; set; }
        public string Skill { get; set; }

        public sealed class UpsertSkillCommandHandler : IRequestHandler<UpsertSkillCommand, Unit>
        {
            private readonly ISSWRewardsDbContext _context;

            public UpsertSkillCommandHandler(ISSWRewardsDbContext context)
            {
                _context = context;
            }

            public async Task<Unit> Handle(UpsertSkillCommand request, CancellationToken cancellationToken)
            {
                if (!string.IsNullOrWhiteSpace(request.Skill))
                {
                    var found = _context.Skills.FirstOrDefault(x => x.Id == request.Id);

                    if (found == null)
                    {
                        _context.Skills.Add(new Domain.Entities.Skill 
                        { 
                            Name = request.Skill, 
                            CreatedUtc = DateTime.Now 
                        });
                    }
                    else
                    {
                        found.Name = request.Skill;
                    }

                    await _context.SaveChangesAsync(cancellationToken);
                    return Unit.Value;
                }

                throw new Exception("Bad Request");
            }
        }
    }
}

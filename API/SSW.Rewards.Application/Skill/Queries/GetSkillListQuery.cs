using MediatR;
using Microsoft.EntityFrameworkCore;
using SSW.Rewards.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.Rewards.Application.Skill.Queries
{
    public class GetSkillListQuery : IRequest<SkillListViewModel>
    {
        public sealed class Handler : IRequestHandler<GetSkillListQuery, SkillListViewModel>
        {
            private readonly ISSWRewardsDbContext _dbContext;

            public Handler(ISSWRewardsDbContext dbContext)
            {
                _dbContext = dbContext;
            }

            public async Task<SkillListViewModel> Handle(GetSkillListQuery request, CancellationToken cancellationToken)
            {
                var skills = await _dbContext.Skills.Select(x => x.Name)
                    .ToListAsync();

                var skillListViewModel = new SkillListViewModel 
                { 
                    Skills = skills 
                };

                return skillListViewModel;
            }
        }
    }
}

using MediatR;
using Microsoft.EntityFrameworkCore;
using SSW.Rewards.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.Rewards.Application.Skills.Queries.GetAdminSkillList
{
    public class GetAdminSkillListQuery : IRequest<List<AdminSkill>>
    {
        public sealed class GetAdminSkillListQueryHandler : IRequestHandler<GetAdminSkillListQuery, List<AdminSkill>>
        {
            private readonly ISSWRewardsDbContext _dbContext;

            public GetAdminSkillListQueryHandler(ISSWRewardsDbContext dbContext)
            {
                _dbContext = dbContext;
            }

            public async Task<List<AdminSkill>> Handle(GetAdminSkillListQuery request, CancellationToken cancellationToken) 
                => await _dbContext.Skills.Select(x => new AdminSkill { Id = x.Id, Name = x.Name })
                   .ToListAsync();
        }
    }
}

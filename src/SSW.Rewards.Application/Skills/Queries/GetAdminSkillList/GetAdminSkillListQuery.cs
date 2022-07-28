using MediatR;
using Microsoft.EntityFrameworkCore;
using SSW.Rewards.Application.Common.Interfaces;

namespace SSW.Rewards.Application.Skills.Queries.GetAdminSkillList;

public class GetAdminSkillListQuery : IRequest<List<AdminSkill>>
{
    public sealed class GetAdminSkillListQueryHandler : IRequestHandler<GetAdminSkillListQuery, List<AdminSkill>>
    {
        private readonly IApplicationDbContext _dbContext;

        public GetAdminSkillListQueryHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<AdminSkill>> Handle(GetAdminSkillListQuery request, CancellationToken cancellationToken) 
            => await _dbContext.Skills.Select(x => new AdminSkill { Id = x.Id, Name = x.Name })
               .ToListAsync();
    }
}

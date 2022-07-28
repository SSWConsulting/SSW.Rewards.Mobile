using MediatR;
using Microsoft.EntityFrameworkCore;
using SSW.Rewards.Application.Common.Interfaces;

namespace SSW.Rewards.Application.Skills.Queries.GetAdminSkillList;
public class GetAdminSkillList : IRequest<List<AdminSkillDto>>
{
    
}

public class GetAdminSkillListHandler : IRequestHandler<GetAdminSkillList, List<AdminSkillDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetAdminSkillListHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<AdminSkillDto>> Handle(GetAdminSkillList request, CancellationToken cancellationToken)
    {
        return await _dbContext.Skills
                                .Select(x => new AdminSkillDto { Id = x.Id, Name = x.Name })
                                .ToListAsync();
    }
}
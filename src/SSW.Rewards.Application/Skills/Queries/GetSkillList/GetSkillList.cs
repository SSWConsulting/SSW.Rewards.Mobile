using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SSW.Rewards.Application.Common.Interfaces;

namespace SSW.Rewards.Application.Skills.Queries.GetSkillList;
public class GetSkillList : IRequest<SkillListViewModel>
{
    
}

public sealed class GetSkillListHandler : IRequestHandler<GetSkillList, SkillListViewModel>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IMapper _mapper;

    public GetSkillListHandler(IApplicationDbContext dbContext, IMapper mapper)
    {
        _dbContext  = dbContext;
        _mapper     = mapper;
    }

    public async Task<SkillListViewModel> Handle(GetSkillList request, CancellationToken cancellationToken)
    {
        var skills = await _dbContext.Skills
                                     .Select(x => x.Name)
                                     .ToListAsync();

        return new SkillListViewModel
        {
            Skills = skills
        };
    }
}
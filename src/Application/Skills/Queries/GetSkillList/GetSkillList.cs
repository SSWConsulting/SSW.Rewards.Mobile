using AutoMapper.QueryableExtensions;
using Shared.DTOs.Skills;

namespace SSW.Rewards.Application.Skills.Queries.GetSkillList;
public class GetSkillList : IRequest<SkillsListViewModel>
{
    
}

public sealed class GetSkillListHandler : IRequestHandler<GetSkillList, SkillsListViewModel>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IMapper _mapper;

    public GetSkillListHandler(IApplicationDbContext dbContext, IMapper mapper)
    {
        _dbContext  = dbContext;
        _mapper     = mapper;
    }

    public async Task<SkillsListViewModel> Handle(GetSkillList request, CancellationToken cancellationToken)
    {
        var skills = await _dbContext.Skills
            .ProjectTo<SkillDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return new SkillsListViewModel
        {
            Skills = skills
        };
    }
}
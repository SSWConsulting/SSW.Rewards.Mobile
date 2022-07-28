using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SSW.Rewards.Application.Common.Interfaces;

namespace SSW.Rewards.Application.Skills.Queries;

public class GetSkillListQuery : IRequest<SkillListViewModel>
{
    public sealed class Handler : IRequestHandler<GetSkillListQuery, SkillListViewModel>
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly IMapper _mapper;

        public Handler(IApplicationDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
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

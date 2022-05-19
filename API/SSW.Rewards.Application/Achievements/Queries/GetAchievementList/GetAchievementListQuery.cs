using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SSW.Rewards.Application.Common.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.Rewards.Application.Achievements.Queries.GetAchievementList
{
    public class GetAchievementListQuery : IRequest<AchievementListViewModel>
    {
        public sealed class GetAchievementListQueryHandler : IRequestHandler<GetAchievementListQuery, AchievementListViewModel>
        {
            private readonly IMapper _mapper;
            private readonly ISSWRewardsDbContext _context;

            public GetAchievementListQueryHandler(
                IMapper mapper,
                ISSWRewardsDbContext context)
            {
                _mapper = mapper;
                _context = context;
            }

            public async Task<AchievementListViewModel> Handle(GetAchievementListQuery request, CancellationToken cancellationToken)
            {
                var achievements = await _context
                    .Achievements
                    .ProjectTo<AchievementViewModel>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);

                return new AchievementListViewModel
                {
                    Achievements = achievements
                };
            }
        }
    }
}

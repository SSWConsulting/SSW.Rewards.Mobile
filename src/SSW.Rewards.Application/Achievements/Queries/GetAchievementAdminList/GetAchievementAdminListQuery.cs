using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SSW.Rewards.Application.Common.Interfaces;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.Rewards.Application.Achievements.Queries.GetAchievementAdminList
{
    public class GetAchievementAdminListQuery : IRequest<AchievementAdminListViewModel>
    {
        public bool IncludeArchived { get; set; }
        
        public sealed class GetAchievementListQueryHandler : IRequestHandler<GetAchievementAdminListQuery, AchievementAdminListViewModel>
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
            public async Task<AchievementAdminListViewModel> Handle(GetAchievementAdminListQuery request, CancellationToken cancellationToken)
            {
                var results = await _context
                    .Achievements
                    .Where(a => request.IncludeArchived || !a.IsDeleted)
                    .ProjectTo<AchievementAdminViewModel>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);

                return new AchievementAdminListViewModel
                {
                    Achievements = results
                };
            }
        }
    }
}

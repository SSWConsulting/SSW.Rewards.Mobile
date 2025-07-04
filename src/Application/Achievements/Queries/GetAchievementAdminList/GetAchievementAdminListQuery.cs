using AutoMapper.QueryableExtensions;
using SSW.Rewards.Shared.DTOs.Achievements;

namespace SSW.Rewards.Application.Achievements.Queries.GetAchievementAdminList;

public class GetAchievementAdminListQuery : IRequest<AchievementAdminListViewModel>
{
    public bool IncludeArchived { get; set; }

    public sealed class GetAchievementListQueryHandler : IRequestHandler<GetAchievementAdminListQuery, AchievementAdminListViewModel>
    {
        private readonly IMapper _mapper;
        private readonly IApplicationDbContext _context;

        public GetAchievementListQueryHandler(IMapper mapper, IApplicationDbContext context)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<AchievementAdminListViewModel> Handle(GetAchievementAdminListQuery request, CancellationToken cancellationToken)
        {
            IQueryable<Achievement> query = _context.Achievements
                .AsNoTracking()
                .TagWithContext();

            if (request.IncludeArchived)
            {
                // Ignore global filter `DeletedUtc == null`
                query = query.IgnoreQueryFilters();
            }

            var results = await query
                .ProjectTo<AchievementAdminDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return new AchievementAdminListViewModel
            {
                Achievements = results
            };
        }
    }
}

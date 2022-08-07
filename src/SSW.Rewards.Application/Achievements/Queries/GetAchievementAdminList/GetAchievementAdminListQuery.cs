using AutoMapper.QueryableExtensions;

namespace SSW.Rewards.Application.Achievements.Queries.GetAchievementAdminList;

public class GetAchievementAdminListQuery : IRequest<AchievementAdminListViewModel>
{
    public bool IncludeArchived { get; set; }
    
    public sealed class GetAchievementListQueryHandler : IRequestHandler<GetAchievementAdminListQuery, AchievementAdminListViewModel>
    {
        private readonly IMapper _mapper;
        private readonly IApplicationDbContext _context;

        public GetAchievementListQueryHandler(
            IMapper mapper,
            IApplicationDbContext context)
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

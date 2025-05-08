using AutoMapper.QueryableExtensions;
using SSW.Rewards.Shared.DTOs.Achievements;

namespace SSW.Rewards.Application.Achievements.Queries.SearchAchievements;

public class SearchAchievementQuery : IRequest<AchievementListViewModel>, IPagedRequest
{
    public string SearchTerm { get; set; } = string.Empty;
    public int Page { get; set; }
    public int PageSize { get; set; }
}

public class SearchAchievementQueryHandler : IRequestHandler<SearchAchievementQuery, AchievementListViewModel>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public SearchAchievementQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<AchievementListViewModel> Handle(SearchAchievementQuery request, CancellationToken cancellationToken)
    {
        string searchTerm = request.SearchTerm?.ToLower() ?? string.Empty;
        var achievements = await _context.Achievements
            .AsNoTracking()
            .TagWithContext()
            .WhenStringNotEmpty(searchTerm, x => x.Name != null && x.Name.Contains(searchTerm))
            .ProjectTo<AchievementDto>(_mapper.ConfigurationProvider)
            .ApplyPagination(request)
            .ToListAsync(cancellationToken);

        return new AchievementListViewModel { Achievements = achievements };
    }
}

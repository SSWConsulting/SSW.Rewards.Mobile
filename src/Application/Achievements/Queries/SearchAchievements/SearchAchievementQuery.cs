using SSW.Rewards.Shared.DTOs.Achievements;

namespace SSW.Rewards.Application.Achievements.Queries.SearchAchievements;

public class SearchAchievementQuery : IRequest<AchievementListViewModel>
{
    public string SearchTerm { get; set; } = string.Empty;
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
        if (string.IsNullOrWhiteSpace(request?.SearchTerm))
        {
            return new() { Achievements = [] };
        }

        string searchTerm = request.SearchTerm.ToLower();
        var achievements = await _context.Achievements
            .Where(x => x.Name != null && x.Name.Contains(searchTerm))
            .ToListAsync(cancellationToken);

        return new AchievementListViewModel
        {
            Achievements = _mapper.Map<IEnumerable<AchievementDto>>(achievements)
        };
    }
}

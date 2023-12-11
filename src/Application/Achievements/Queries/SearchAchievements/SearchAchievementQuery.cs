using SSW.Rewards.Application.Achievements.Queries.Common;

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
        var achievements = await _context.Achievements
            .Where(a => a.Name.ToLower().Contains(request.SearchTerm.ToLower()))
            .ToListAsync(cancellationToken);

        return new AchievementListViewModel
        {
            Achievements = _mapper.Map<IEnumerable<AchievementDto>>(achievements)
        };
    }
}

using SSW.Rewards.Shared.DTOs.Rewards;

namespace SSW.Rewards.Application.Rewards.Queries.SearchRewards;

public class SearchRewardsQuery : IRequest<RewardListViewModel>
{
    public string SearchTerm { get; set; } = string.Empty;
}

public class SearchRewardsQueryHandler : IRequestHandler<SearchRewardsQuery, RewardListViewModel>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public SearchRewardsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<RewardListViewModel> Handle(SearchRewardsQuery request, CancellationToken cancellationToken)
    {
        var rewards = await _context.Rewards
            .Where(a => a.Name.ToLower().Contains(request.SearchTerm.ToLower()))
            .ToListAsync(cancellationToken);

        return new RewardListViewModel
        {
            Rewards = _mapper.Map<IEnumerable<RewardDto>>(rewards)
        };
    }
}

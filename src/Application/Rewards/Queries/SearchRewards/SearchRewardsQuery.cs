using AutoMapper.QueryableExtensions;
using SSW.Rewards.Shared.DTOs.Rewards;

namespace SSW.Rewards.Application.Rewards.Queries.SearchRewards;

public class SearchRewardsQuery : IRequest<RewardListViewModel>, IPagedRequest
{
    public string SearchTerm { get; set; } = string.Empty;

    public int Page { get; set; }

    public int PageSize { get; set; }
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
        string searchTerm = request.SearchTerm?.ToLower() ?? string.Empty;
        var rewards = await _context.Rewards
            .AsNoTracking()
            .TagWithContext()
            .WhenStringNotEmpty(searchTerm, x => x.Name != null && x.Name.Contains(searchTerm))
            .ProjectTo<RewardDto>(_mapper.ConfigurationProvider)
            .ApplyPagination(request)
            .ToListAsync(cancellationToken);

        return new RewardListViewModel { Rewards = rewards };
    }
}

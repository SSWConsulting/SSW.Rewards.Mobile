using AutoMapper.QueryableExtensions;
using Shared.DTOs.Rewards;

namespace SSW.Rewards.Application.Rewards.Queries.GetRecentRewards;

public class GetRecentRewardsQuery : IRequest<RecentRewardListViewModel>
{
    public DateTime? Since { get; set; }
}

public class GetRecentRewardsQueryHandler : IRequestHandler<GetRecentRewardsQuery, RecentRewardListViewModel>
{
    private IMapper _mapper;
    private IApplicationDbContext _context;

    public GetRecentRewardsQueryHandler(
        IMapper mapper,
        IApplicationDbContext context)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<RecentRewardListViewModel> Handle(GetRecentRewardsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.UserRewards.Where(u => u.Id != 0);

        if (!(request.Since == null))
        {
            query = query
                .Where(u => u.AwardedAt > request.Since.Value.ToUniversalTime());
        }
        else
        {
            query = query
                .OrderByDescending(u => u.AwardedAt)
                .Take(10);
        }

        var results = await query
            .ProjectTo<RecentRewardDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return new RecentRewardListViewModel
        {
            RecentRewards = results
        };
    }
}
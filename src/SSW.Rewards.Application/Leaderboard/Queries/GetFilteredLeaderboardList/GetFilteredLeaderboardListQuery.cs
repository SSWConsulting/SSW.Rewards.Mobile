using AutoMapper.QueryableExtensions;
using SSW.Rewards.Application.Leaderboard.Queries.Common;

namespace SSW.Rewards.Application.Leaderboard.Queries.GetFilteredLeaderboardList;

public class GetFilteredLeaderboardListQuery : IRequest<LeaderboardListViewModel>
{
    public LeaderboardFilter Filter { get; set; }
}

public class GetFilteredLeaderboardListQueryHandler : IRequestHandler<GetFilteredLeaderboardListQuery, LeaderboardListViewModel>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetFilteredLeaderboardListQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<LeaderboardListViewModel> Handle(GetFilteredLeaderboardListQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Users
            .Where(u => u.Activated == true);

        if (request.Filter == LeaderboardFilter.ThisYear)
        {
            query = query.Where(u => u.UserAchievements.Any(a => a.AwardedAt.Year == DateTime.Now.Year));
        }
        else if (request.Filter == LeaderboardFilter.ThisMonth) // and year
        {
            query = query.Where(u => u.UserAchievements.Any(a => a.AwardedAt.Month == DateTime.Now.Month));
        }

        var users = await query.Include(u => u.UserAchievements)
            .ThenInclude(ua => ua.Achievement)
            .ProjectTo<LeaderboardUserDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        var model = new LeaderboardListViewModel
        {
            // need to set rank outside of AutoMapper
            Users = users
                    .Where(u => !string.IsNullOrWhiteSpace(u.Name))
                    .OrderByDescending(u => u.TotalPoints)
                    .Select((u, i) =>
                    {
                        u.Rank = i + 1;
                        return u;
                    }).ToList()
        };

        return model;
    }
}

public enum LeaderboardFilter
{
    ThisMonth,
    ThisYear
}

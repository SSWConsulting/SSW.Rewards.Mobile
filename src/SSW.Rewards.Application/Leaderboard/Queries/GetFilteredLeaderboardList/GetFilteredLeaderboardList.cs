using AutoMapper.QueryableExtensions;
using SSW.Rewards.Application.Leaderboard.Queries.Common;

namespace SSW.Rewards.Application.Leaderboard.Queries.GetFilteredLeaderboardList;
public class GetFilteredLeaderboardList : IRequest<LeaderboardListViewModel>
{
    public LeaderboardFilter Filter { get; set; }

    public GetFilteredLeaderboardList(LeaderboardFilter filter)
    {
        Filter = filter;
    }
}

public class GetFilteredLeaderboardListHandler : IRequestHandler<GetFilteredLeaderboardList, LeaderboardListViewModel>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IDateTime _dateTime;

    public GetFilteredLeaderboardListHandler(
        IApplicationDbContext context,
        IMapper mapper,
        IDateTime dateTime)
    {
        _context = context;
        _mapper = mapper;
        _dateTime = dateTime;
    }

    public async Task<LeaderboardListViewModel> Handle(GetFilteredLeaderboardList request, CancellationToken cancellationToken)
    {
        var query = _context.Users
            .Where(u => u.Activated == true);

        if (request.Filter == LeaderboardFilter.ThisYear)
        {
            query = query.Where(u => u.UserAchievements.Any(a => a.AwardedAt.Year == _dateTime.Now.Year));
        }
        else if (request.Filter == LeaderboardFilter.ThisMonth) // and year
        {
            query = query.Where(u => u.UserAchievements.Any(a => a.AwardedAt.Month == _dateTime.Now.Month));
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



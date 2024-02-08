using AutoMapper.QueryableExtensions;
using SSW.Rewards.Shared.DTOs.Leaderboard;
using SSW.Rewards.Application.Common.Extensions;

namespace SSW.Rewards.Application.Leaderboard.Queries.GetFilteredLeaderboardList;
public class GetFilteredLeaderboardList : IRequest<LeaderboardViewModel>
{
    public LeaderboardFilter Filter { get; set; }

    public GetFilteredLeaderboardList(LeaderboardFilter filter)
    {
        Filter = filter;
    }
}

public class GetFilteredLeaderboardListHandler : IRequestHandler<GetFilteredLeaderboardList, LeaderboardViewModel>
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

    public async Task<LeaderboardViewModel> Handle(GetFilteredLeaderboardList request, CancellationToken cancellationToken)
    {
        var query = _context.Users
            .Where(u => u.Activated == true);

        if (request.Filter == LeaderboardFilter.ThisYear)
        {
            query = query.Where(u => u.UserAchievements.Any(a => a.AwardedAt.Year == _dateTime.UtcNow.Year));
        }
        else if (request.Filter == LeaderboardFilter.ThisMonth)
        {
            query = query.Where(u => u.UserAchievements.Any(a => a.AwardedAt.Year == _dateTime.UtcNow.Year && a.AwardedAt.Month == _dateTime.UtcNow.Month));
        }
        else if (request.Filter == LeaderboardFilter.Today)
        {
            query = query.Where(u => u.UserAchievements.Any(a => a.AwardedAt.Year == _dateTime.UtcNow.Year && a.AwardedAt.Month == _dateTime.UtcNow.Month && a.AwardedAt.Day == _dateTime.UtcNow.Day));
        }
        else if (request.Filter == LeaderboardFilter.ThisWeek)
        {
            var start = _dateTime.UtcNow.FirstDayOfWeek();
            var end = start.AddDays(7);
            // TODO: Find a better way - EF Can't translate our extension method -- so writing the date range comparison directly in linq for now
            query = query.Where(u => u.UserAchievements.Any(a => start <= a.AwardedAt && a.AwardedAt <= end));
        }

        var users = await query.Include(u => u.UserAchievements)
            .ThenInclude(ua => ua.Achievement)
            .ProjectTo<LeaderboardUserDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        var model = new LeaderboardViewModel
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



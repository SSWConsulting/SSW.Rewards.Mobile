using MoreLinq;
using SSW.Rewards.Shared.DTOs.Leaderboard;
using SSW.Rewards.Application.Common.Extensions;

namespace SSW.Rewards.Application.Leaderboard.Queries.GetFilteredLeaderboardList;

public class GetFilteredLeaderboardListQuery : IRequest<LeaderboardViewModel>
{
    public LeaderboardFilter Filter { get; set; }
}

public class GetFilteredLeaderboardListQueryHandler : IRequestHandler<GetFilteredLeaderboardListQuery, LeaderboardViewModel>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTime _dateTime;

    public GetFilteredLeaderboardListQueryHandler(
        IApplicationDbContext context,
        IDateTime dateTime)
    {
        _context = context;
        _dateTime = dateTime;
    }

    public async Task<LeaderboardViewModel> Handle(GetFilteredLeaderboardListQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Users
            .Where(u => u.Activated == true);

        if (request.Filter == LeaderboardFilter.ThisYear)
        {
            query = query.Where(u => u.UserAchievements.Any(a => a.AwardedAt.Year == _dateTime.Now.Year));
        }
        else if (request.Filter == LeaderboardFilter.ThisMonth)
        {
            query = query.Where(u => u.UserAchievements.Any(a => a.AwardedAt.Year == _dateTime.Now.Year && a.AwardedAt.Month == _dateTime.Now.Month));
        }
        else if (request.Filter == LeaderboardFilter.Today)
        {
            query = query.Where(u => u.UserAchievements.Any(a => a.AwardedAt.Year == _dateTime.Now.Year && a.AwardedAt.Month == _dateTime.Now.Month && a.AwardedAt.Day == _dateTime.Now.Day));
        }
        else if (request.Filter == LeaderboardFilter.ThisWeek)
        {
            var start = _dateTime.Now.FirstDayOfWeek();
            var end = start.AddDays(7);
            // TODO: Find a better way - EF Can't translate our extension method -- so writing the date range comparison directly in linq for now
            query = query.Where(u => u.UserAchievements.Any(a => start <= a.AwardedAt && a.AwardedAt <= end));
        }

        var users = await query
            .Include(u => u.UserAchievements)
            .ThenInclude(ua => ua.Achievement)
            .Where(u => !string.IsNullOrWhiteSpace(u.FullName))
            .Select(u => new LeaderboardUserDto(u))
            .ToListAsync(cancellationToken);
        
        users
            .OrderByDescending(lud => lud.TotalPoints)
            .ForEach((u, i) => u.Rank = i + 1);

        var model = new LeaderboardViewModel { Users = users };

        return model;
    }
}
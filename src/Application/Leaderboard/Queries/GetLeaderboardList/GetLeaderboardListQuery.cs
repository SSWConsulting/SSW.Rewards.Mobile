using MoreLinq;
using SSW.Rewards.Application.Common.Extensions;
using SSW.Rewards.Shared.DTOs.Leaderboard;

namespace SSW.Rewards.Application.Leaderboard.Queries.GetLeaderboardList;

public class GetLeaderboardListQuery : IRequest<LeaderboardViewModel>
{

}

public class Handler : IRequestHandler<GetLeaderboardListQuery, LeaderboardViewModel>
{
    private readonly IApplicationDbContext _context;

    public Handler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<LeaderboardViewModel> Handle(GetLeaderboardListQuery request, CancellationToken cancellationToken)
    {
        var users = await _context.Users
            .Where(u => u.Activated)
            .Include(u => u.UserAchievements)
                .ThenInclude(ua => ua.Achievement)
            .Include(u => u.SocialMediaIds)
                .ThenInclude(s => s.SocialMediaPlatform)
            .Where(u => !string.IsNullOrWhiteSpace(u.FullName))
            .Select(u => new LeaderboardUserDto(u, DateTime.Now.FirstDayOfWeek()))
            .ToListAsync(cancellationToken);

        users
            .OrderByDescending(lud => lud.TotalPoints)
            .ForEach((u, i) => u.Rank = i + 1);
        
        var model = new LeaderboardViewModel { Users = users };

        return model;
    }
}

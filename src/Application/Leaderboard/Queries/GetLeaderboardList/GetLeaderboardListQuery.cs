using System.Text.RegularExpressions;
using SSW.Rewards.Shared.DTOs.Leaderboard;

namespace SSW.Rewards.Application.Leaderboard.Queries.GetLeaderboardList;

public class GetLeaderboardListQuery : IRequest<LeaderboardViewModel> { }

public class Handler : IRequestHandler<GetLeaderboardListQuery, LeaderboardViewModel>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTime _dateTime;
    private readonly ICacheService _cacheService;

    public Handler(IApplicationDbContext context, IDateTime dateTime, ICacheService cacheService)
    {
        _context = context;
        _dateTime = dateTime;
        _cacheService = cacheService;
    }

    public async Task<LeaderboardViewModel> Handle(GetLeaderboardListQuery request, CancellationToken cancellationToken)
    {
        var users = await _cacheService.GetOrAddAsync(CacheKeys.Leaderboard, () => GenerateLeaderboard(cancellationToken));

        return new LeaderboardViewModel { Users = users };
    }

    private async Task<List<LeaderboardUserDto>> GenerateLeaderboard(CancellationToken cancellationToken)
    {
        DateTime utcNow = _dateTime.UtcNow;

        var users = await _context.Users
            .AsNoTracking()
            .AsSplitQuery()
            .TagWithContext()
            .Where(x => x.Activated && !string.IsNullOrWhiteSpace(x.FullName))
            .Select(x => new LeaderboardUserDto
            {
                UserId = x.Id,
                Name = x.FullName,
                Email = x.Email,
                ProfilePic = x.Avatar,
                PointsClaimed = x.UserRewards.Sum(ur => ur.Reward.Cost),
                TotalPoints = x.UserAchievements.Sum(ua => ua.Achievement.Value),
                PointsToday = x.UserAchievements
                    .Where(ua =>
                        ua.AwardedAt.Year == utcNow.Year &&
                        ua.AwardedAt.Month == utcNow.Month &&
                        ua.AwardedAt.Day == utcNow.Day)
                    .Sum(ua => ua.Achievement.Value),
                PointsThisWeek = x.UserAchievements
                    .Where(ua => utcNow.AddDays(-7) <= ua.AwardedAt && ua.AwardedAt <= utcNow)
                    .Sum(ua => ua.Achievement.Value),
                PointsThisMonth = x.UserAchievements
                    .Where(ua => ua.AwardedAt.Year == utcNow.Year && ua.AwardedAt.Month == utcNow.Month)
                    .Sum(ua => ua.Achievement.Value),
                PointsThisYear = x.UserAchievements
                    .Where(ua => ua.AwardedAt.Year == utcNow.Year)
                    .Sum(ua => ua.Achievement.Value),
                Title = x.SocialMediaIds
                    .Where(s => s.SocialMediaPlatform.Name == "Company")
                    .Select(s => s.SocialMediaUserId)
                    .FirstOrDefault()
                    ?? ""
            })
            .ToListAsync(cancellationToken);

        // Post-processing
        int rank = 0;
        Regex checkHttpRegex = new(@"^https?://", RegexOptions.Compiled, TimeSpan.FromMilliseconds(100));
        foreach (LeaderboardUserDto? user in users.OrderByDescending(lud => lud.TotalPoints))
        {
            user.Rank = ++rank;
            user.Title = user.Title switch
            {
                _ when !string.IsNullOrEmpty(user.Title) => checkHttpRegex.Replace(user.Title, string.Empty),
                _ when user.Email?.EndsWith("ssw.com.au") == true => "SSW",
                _ => "Community"
            };
        }

        return users;
    }
}

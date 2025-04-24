using System.Text.RegularExpressions;
using SSW.Rewards.Shared.DTOs.Leaderboard;

namespace SSW.Rewards.Application.Leaderboard.Queries.GetLeaderboardPaginatedList;

public class GetLeaderboardPaginatedListQuery : IRequest<LeaderboardViewModel> 
{
    public int Skip { get; set; }
    public int Take { get; set; }
    public LeaderboardFilter currentPeriod { get; set; }
}

public class Handler : IRequestHandler<GetLeaderboardPaginatedListQuery, LeaderboardViewModel>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTime _dateTime;
    private readonly IProfilePicStorageProvider _profilePicStorageProvider;

    public Handler(IApplicationDbContext context, IDateTime dateTime, IProfilePicStorageProvider profilePicStorageProvider)
    {
        _context = context;
        _dateTime = dateTime;
        _profilePicStorageProvider = profilePicStorageProvider;
    }

    public async Task<LeaderboardViewModel> Handle(GetLeaderboardPaginatedListQuery request, CancellationToken cancellationToken)
    {
        var skip = request.Skip;
        var take = request.Take;
        var currentPeriod = request.currentPeriod;

        var users = await GenerateLeaderboard(take, skip, currentPeriod, cancellationToken);

        return new LeaderboardViewModel { Users = users };
    }

    private async Task<List<LeaderboardUserDto>> GenerateLeaderboard(int take, int skip, LeaderboardFilter currentPeriod, CancellationToken cancellationToken)
    {
        DateTime utcNow = _dateTime.UtcNow;

        var query = _context.Users
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
            });
        
        switch (currentPeriod) {
            case LeaderboardFilter.ThisMonth:
                query = query.OrderByDescending(x => x.PointsThisMonth);
                break;
            case LeaderboardFilter.ThisYear:
                query = query.OrderByDescending(x => x.PointsThisYear);
                break;
            case LeaderboardFilter.Forever:
                query = query.OrderByDescending(x => x.TotalPoints);
                break;
            case LeaderboardFilter.ThisWeek:
                query = query.OrderByDescending(x => x.PointsThisWeek);
                break;
            default:
                query = query.OrderByDescending(x => x.TotalPoints);
                break;
        }

        var users = await query
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);

        var defaultProfilePictureUrl = await _profilePicStorageProvider.GetProfilePicUri("v2sophie.png");

        // Post-processing
        int rank = 0;
        Regex checkHttpRegex = new(@"^https?://", RegexOptions.Compiled, TimeSpan.FromMilliseconds(100));
        foreach (LeaderboardUserDto? user in users)
        {
            user.Rank = ++rank;
            user.Title = user.Title switch
            {
                _ when !string.IsNullOrEmpty(user.Title) => checkHttpRegex.Replace(user.Title, string.Empty),
                _ when user.Email?.EndsWith("ssw.com.au") == true => "SSW",
                _ => "Community"
            };

            if (string.IsNullOrEmpty(user.ProfilePic) && defaultProfilePictureUrl != null)
            {
                user.ProfilePic = defaultProfilePictureUrl.ToString();
            }
        }

        return users;
    }
}

using SSW.Rewards.Shared.DTOs.Leaderboard;
using SSW.Rewards.Shared.Utils;

namespace SSW.Rewards.Application.Leaderboard;

public interface ILeaderboardService
{
    Task<List<LeaderboardUserDto>> GetFullLeaderboard(CancellationToken cancellationToken);
    Task<LeaderboardUserDto?> GetUserById(int userId, CancellationToken cancellationToken);
}

public class LeaderboardService : ILeaderboardService
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTime _dateTime;
    private readonly ICacheService _cacheService;
    private readonly IProfilePicStorageProvider _profilePicStorageProvider;

    public LeaderboardService(IApplicationDbContext context, IDateTime dateTime, ICacheService cacheService, IProfilePicStorageProvider profilePicStorageProvider)
    {
        _context = context;
        _dateTime = dateTime;
        _cacheService = cacheService;
        _profilePicStorageProvider = profilePicStorageProvider;
    }

    public async Task<LeaderboardUserDto?> GetUserById(int userId, CancellationToken cancellationToken)
    {
        var leaderboard = await GetFullLeaderboard(cancellationToken);
        return leaderboard.FirstOrDefault(u => u.UserId == userId);
    }

    public async Task<List<LeaderboardUserDto>> GetFullLeaderboard(CancellationToken cancellationToken)
    {
        var users = await _cacheService.GetOrAddAsync(CacheKeys.Leaderboard, () => GenerateLeaderboard(cancellationToken));

        return users;
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

        var defaultProfilePictureUrl = await _profilePicStorageProvider.GetProfilePicUri("v2sophie.png");

        // Post-processing
        int rank = 0;
        foreach (LeaderboardUserDto? user in users.OrderByDescending(lud => lud.TotalPoints))
        {
            user.Rank = ++rank;
            user.Title = user.Title switch
            {
                _ when !string.IsNullOrEmpty(user.Title) => RegexHelpers.TitleRegex().Replace(user.Title, string.Empty),
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

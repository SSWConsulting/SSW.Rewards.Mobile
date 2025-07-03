using SSW.Rewards.Application.Leaderboard;
using SSW.Rewards.Application.System.Commands.Common;

namespace SSW.Rewards.Application.Achievements.Notifications;

public class UserMilestoneAchievementCheckRequested : INotification
{
    public int UserId { get; set; }
}

public class UserMilestoneAchievementCheckRequestedHandler : INotificationHandler<UserMilestoneAchievementCheckRequested>
{
    private readonly IApplicationDbContext _context;
    private readonly ILeaderboardService _leaderboardService;
    private readonly ICacheService _cacheService;

    public UserMilestoneAchievementCheckRequestedHandler(IApplicationDbContext context, ILeaderboardService leaderboardService, ICacheService cacheService)
    {
        _context = context;
        _leaderboardService = leaderboardService;
        _cacheService = cacheService;
    }

    public async Task Handle(UserMilestoneAchievementCheckRequested notification, CancellationToken cancellationToken)
    {
        // Remove cache to update ranking cache.
        _cacheService.Remove(CacheTags.UpdatedRanking);

        int userId = notification.UserId;

        // Check if user is eligible for MilestoneAchievements.Leaderboard
        var userRank = await _leaderboardService.GetUserById(userId, cancellationToken);
        if (userRank != null && userRank.Rank <= MilestoneAchievements.LeaderboardTopUsersNumber)
        {
            int leaderboardTop100AchievementId = await _cacheService.GetOrAddAsync(
                CacheKeys.LeaderboardTop100AchievementId,
                () => GetLeaderboardTop100AchievementId(cancellationToken));
            if (leaderboardTop100AchievementId > 0)
            {
                var userHasTopLeaderboardAchievement = await _context.UserAchievements
                    .AsNoTracking()
                    .AnyAsync(ua => ua.UserId == userId && ua.AchievementId == leaderboardTop100AchievementId, cancellationToken);
                if (!userHasTopLeaderboardAchievement)
                {
                    _context.UserAchievements.Add(new UserAchievement
                    {
                        UserId = userId,
                        AchievementId = leaderboardTop100AchievementId
                    });
                    await _context.SaveChangesAsync(cancellationToken);
                }
            }
        }
    }

    private async Task<int> GetLeaderboardTop100AchievementId(CancellationToken ct)
    {
        var achievement = await _context.Achievements
            .AsNoTracking()
            .TagWithContext("GetLeaderboardTop100Achievement")
            .Where(a => a.Name == MilestoneAchievements.LeaderboardTopUsers)
            .Select(x => new { x.Id })
            .FirstOrDefaultAsync(ct);

        return achievement?.Id ?? -1;
    }
}

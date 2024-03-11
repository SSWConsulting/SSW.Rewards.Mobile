using SSW.Rewards.Shared.DTOs.ActivityFeed;
using SSW.Rewards.Shared.DTOs.Users;

namespace SSW.Rewards.Application.Services;

public class ActivityFeedService : IActivityFeedService
{
    private readonly IApplicationDbContext _dbContext;

    public ActivityFeedService(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IList<ActivityFeedViewModel>> GetActivities(ActivityFeedFilter filter, int skip, int take, CancellationToken cancellationToken)
    {
        // TODO: get friends and filter

        var userAchievements = await _dbContext.UserAchievements
            .OrderByDescending(x => x.AwardedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        var result = new List<ActivityFeedViewModel>(userAchievements.Count);
        foreach (var userAchievement in userAchievements)
        {
            result.Add(new ActivityFeedViewModel
            {
                UserAvatar = userAchievement.User.Avatar,
                UserName = userAchievement.User.FullName,
                Achievement = new UserAchievementDto
                {
                    AchievementName = userAchievement.Achievement.Name,
                    AchievementType = userAchievement.Achievement.Type,
                    AchievementValue = userAchievement.Achievement.Value
                },
                AwardedAt = userAchievement.AwardedAt,
            });
        }

        return result;
    }
}
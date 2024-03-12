using SSW.Rewards.Shared.DTOs.ActivityFeed;
using SSW.Rewards.Shared.DTOs.Users;

namespace Microsoft.Extensions.DependencyInjection.ActivityFeed.Queries;

public class GetActivitiesQuery : IRequest<IList<ActivityFeedViewModel>>
{
    public ActivityFeedFilter Filter { get; set; }
    public int Skip { get; set; }
    public int Take { get; set; }
}

public class GetActivitiesQueryHandler : IRequestHandler<GetActivitiesQuery, IList<ActivityFeedViewModel>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetActivitiesQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IList<ActivityFeedViewModel>> Handle(GetActivitiesQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter;
        var skip = request.Skip;
        var take = request.Take;

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



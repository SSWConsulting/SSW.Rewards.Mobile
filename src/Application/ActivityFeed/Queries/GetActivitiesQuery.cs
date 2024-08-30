using SSW.Rewards.Shared.DTOs.ActivityFeed;
using SSW.Rewards.Shared.DTOs.Users;

namespace Microsoft.Extensions.DependencyInjection.ActivityFeed.Queries;

public class GetActivitiesQuery : IRequest<ActivityFeedViewModel>
{
    public ActivityFeedFilter Filter { get; set; }
    public int Skip { get; set; }
    public int Take { get; set; }
}

public class GetActivitiesQueryHandler(IApplicationDbContext dbContext, ICurrentUserService currentUserService) : IRequestHandler<GetActivitiesQuery, ActivityFeedViewModel>
{

    public async Task<ActivityFeedViewModel> Handle(GetActivitiesQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter;
        var skip = request.Skip;
        var take = request.Take;
        List<UserAchievement> userAchievements;

        var userEmail = currentUserService.GetUserEmail();
        var user = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == userEmail, cancellationToken);

        var staffDetails = await dbContext.StaffMembers
            .Where(s => !s.IsDeleted && s.StaffAchievement != null)
            .Select((s) => new
            {
                s.Email,
                s.Title,
                AchievmentId = s.StaffAchievement!.Id
            })
            .ToListAsync(cancellationToken);

        var staffAchievementIds = staffDetails.Select(s => s.AchievmentId).ToList();

        if (filter == ActivityFeedFilter.Friends)
        {
            List<int> friendIds = [];

            var userAchievementIds = await dbContext.Users
                .Where(u => u.AchievementId.HasValue)
                .Select(u => u.AchievementId)
                .ToListAsync(cancellationToken);

            var haveScannedIds = await dbContext.UserAchievements
                .Where(ua => userAchievementIds.Contains(ua.AchievementId))
                .Select(ua => ua.UserId)
                .ToListAsync(cancellationToken);

            friendIds.AddRange(haveScannedIds);

            var staffScannedIds = await dbContext.UserAchievements
                .Where(ua => staffAchievementIds.Contains(ua.AchievementId) && ua.UserId == user.Id)
                .Select(ua => ua.UserId)
                .ToListAsync(cancellationToken);

            friendIds.AddRange(staffScannedIds);

            var scannedMeIds = await dbContext.UserAchievements
                .Where(ua => ua.AchievementId == user.AchievementId)
                .Include(ua => ua.User)
                .Select(ua => ua.UserId)
                .ToListAsync(cancellationToken);

            friendIds.AddRange(scannedMeIds);

            userAchievements = await dbContext.UserAchievements
                .Include(u => u.User)
                .Include(a => a.Achievement)
                .OrderByDescending(x => x.AwardedAt)
                .Where(x => friendIds.Contains(x.UserId))
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);
        }
        else
        {
            userAchievements = await dbContext.UserAchievements
                .Include(u => u.User)
                .Include(a => a.Achievement)
                .OrderByDescending(x => x.AwardedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);
        }

        var feed = userAchievements.Select(userAchievement =>
        {
            var staff = staffDetails.FirstOrDefault(s => s.Email == userAchievement.User.Email);
            return new ActivityFeedItemDto
            {
                UserAvatar = userAchievement.User.Avatar ?? string.Empty,
                UserName = userAchievement.User.FullName ?? string.Empty,
                UserTitle = staff?.Title ?? "Community",
                UserId = userAchievement.User.Id,
                Achievement = new UserAchievementDto
                {
                    AchievementName = userAchievement.Achievement.Name ?? string.Empty,
                    AchievementType = userAchievement.Achievement.Type,
                    AchievementValue = userAchievement.Achievement.Value
                },
                AwardedAt = userAchievement.AwardedAt,
            };
        }).ToList();

        return new ActivityFeedViewModel()
        {
            Feed = feed
        };
    }
}



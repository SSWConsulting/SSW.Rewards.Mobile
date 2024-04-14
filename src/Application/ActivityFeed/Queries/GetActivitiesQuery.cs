using SSW.Rewards.Shared.DTOs.ActivityFeed;
using SSW.Rewards.Shared.DTOs.Users;

namespace Microsoft.Extensions.DependencyInjection.ActivityFeed.Queries;

public class GetActivitiesQuery : IRequest<ActivityFeedViewModel>
{
    public ActivityFeedFilter Filter { get; set; }
    public int Skip { get; set; }
    public int Take { get; set; }
}

public class GetActivitiesQueryHandler : IRequestHandler<GetActivitiesQuery, ActivityFeedViewModel>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserService _userService;

    public GetActivitiesQueryHandler(IApplicationDbContext dbContext, IUserService userService)
    {
        _dbContext = dbContext;
        _userService = userService;
    }

    public async Task<ActivityFeedViewModel> Handle(GetActivitiesQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter;
        var skip = request.Skip;
        var take = request.Take;
        List<UserAchievement> userAchievements;
        
        var user = await _userService.GetCurrentUser(cancellationToken);
        
        var staffDtos = await _dbContext.StaffMembers
            .Join(_dbContext.Users,
                staff => staff.Email, 
                user => user.Email,
                (staff, user) =>
                    new
                    {
                        UserId = user.Id,
                        staff.Name,
                        ProfilePicture = user.Avatar,
                        staff.Title,
                        staff.Email,
                        AchievementId = staff.StaffAchievement.Id,
                        staff.IsDeleted,
                        user.Activated
                    })
            .Where(x => !x.IsDeleted && x.Activated)
            .ToListAsync(cancellationToken);

        if (filter == ActivityFeedFilter.Friends)
        {
            var completedAchievements = (await _userService.GetUserAchievements(user.Id, cancellationToken))
                .UserAchievements
                .Where(a => a.Complete)
                .Select(a => a.AchievementId)
                .ToList();

            var friendIds = staffDtos
                .Where(staff => staff.UserId != user.Id && completedAchievements.Contains(staff.AchievementId))
                .Select(staff => staff.UserId);

            userAchievements = await _dbContext.UserAchievements
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
            userAchievements = await _dbContext.UserAchievements
                .Include(u => u.User)
                .Include(a => a.Achievement)
                .OrderByDescending(x => x.AwardedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);
        }

        var feed = userAchievements.Select(userAchievement =>
        {
            var staff = staffDtos.FirstOrDefault(s => s.UserId == userAchievement.UserId);
            return new ActivityFeedItemDto
            {
                UserAvatar = userAchievement.User.Avatar ?? string.Empty,
                UserName = userAchievement.User.FullName ?? string.Empty,
                UserTitle = staff?.Title ?? "Community",
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



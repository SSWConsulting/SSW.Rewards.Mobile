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
            .TagWithContext("GetUserByEmail")
            .FirstOrDefaultAsync(u => u.Email == userEmail, cancellationToken);

        var staffDetails = await dbContext.StaffMembers
            .TagWithContext("GetActiveStaffMembers")
            .Where(s => !s.IsDeleted && s.StaffAchievement != null)
            .Select((s) => new
            {
                s.Email,
                s.Title,
                AchievmentId = s.StaffAchievement!.Id
            })
            .ToListAsync(cancellationToken);
        
        var staffUsers = await dbContext.Users
            .TagWithContext("GetStaffBasedOnEmails")
            .Where(u => u.Id != user.Id && staffDetails.Select(s => s.Email).Contains(u.Email))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        if (filter == ActivityFeedFilter.Friends)
        {
            List<int> friendIds = [];

            var allAchievementIds = await dbContext.Users
                .TagWithContext("GetAllUserAchievementsUnionWithLocalStaffDetails")
                .Include(u => u.Achievement)
                .Where(u => u.Achievement != null)
                .Select(u => u.Achievement!.Id)
                .Union(staffDetails.Select(s => s.AchievmentId))
                .ToListAsync(cancellationToken);

            var scannedUserAchievements = await dbContext.UserAchievements
                .TagWithContext("GetUserAchievementsBasedOnAllAchievementsExceptTheirOwnUniqueOnes")
                .Where(ua => ua.UserId == user.Id && allAchievementIds.Contains(ua.AchievementId))
                .Include(ua => ua.User)
                .Select(ua => ua.Achievement.Id)
                .ToListAsync(cancellationToken);
        
            foreach (var scannedUserAchievement in scannedUserAchievements)
            {
                User? userMatch;
                var staffMatch = staffDetails.FirstOrDefault(ua => ua.AchievmentId == scannedUserAchievement);

                if (staffMatch != null)
                {
                    userMatch = staffUsers.FirstOrDefault(u => u.Email == staffMatch.Email);
                }
                else
                {
                    userMatch = await dbContext.Users
                        .TagWithContext("GetUnmatchedStaffMemberForAchievements")
                        .Include(u => u.Achievement)
                        .Where(u => u.Achievement != null)
                        .FirstOrDefaultAsync(ua => ua.Achievement!.Id == scannedUserAchievement, cancellationToken: cancellationToken);
                }
            
                if (userMatch != null)
                {
                    friendIds.Add(userMatch.Id);
                }
            }

            userAchievements = await dbContext.UserAchievements
                .TagWithContext("GetUserAchievementsBasedOnFriends")
                .Include(u => u.User)
                    .ThenInclude(u => u.SocialMediaIds)
                        .ThenInclude(sm => sm.SocialMediaPlatform)
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
                .TagWithContext("GetUserAchievements")
                .Include(u => u.User)
                    .ThenInclude(u => u.SocialMediaIds)
                        .ThenInclude(sm => sm.SocialMediaPlatform)
                .Include(a => a.Achievement)
                .OrderByDescending(x => x.AwardedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);
        }

        var feed = userAchievements.Select(userAchievement =>
        {
            var staff = staffDetails.FirstOrDefault(s => s.Email == userAchievement.User.Email);
            var company = userAchievement.User.SocialMediaIds
                .FirstOrDefault(s => s.SocialMediaPlatform.Name == "Company")?.SocialMediaUserId;
            
            return new ActivityFeedItemDto
            {
                UserAvatar = userAchievement.User.Avatar ?? string.Empty,
                UserName = userAchievement.User.FullName ?? string.Empty,
                UserTitle = staff?.Title ?? company ?? "Community",
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



using SSW.Rewards.Application.Notifications.Commands;
using SSW.Rewards.Application.Notifications.Queries;

namespace SSW.Rewards.Application.Notifications;

public static class NotificationUsersExtensions
{
    public static IQueryable<int> GetTargetNotificationUserIdsQuery(this DbSet<User> userDbSet, SendAdminNotificationCommand request)
        => GetTargetNotificationUserIdsQuery(
            userDbSet,
            request.AchievementIds,
            request.UserIds,
            request.RoleIds);
    
    public static IQueryable<int> GetTargetNotificationUserIdsQuery(this DbSet<User> userDbSet, GetNumberOfImpactedNotificationUsersQuery request)
        => GetTargetNotificationUserIdsQuery(
            userDbSet,
            request.AchievementIds,
            request.UserIds,
            request.RoleIds);
    
    public static IQueryable<int> GetTargetNotificationUserIdsQuery(this DbSet<User> userDbSet, List<int> achievementIds, List<int> userIds, List<int> roleIds)
    {
        IQueryable<User> query = userDbSet
            .AsNoTracking()
            .TagWithContext("NotificationUsers")
            .Where(x => x.Activated);

        if (achievementIds?.Count > 0)
        {
            query = query
                .TagWithContext("ByAchievements")
                .Where(x => x.UserAchievements.Any(a => achievementIds.Contains(a.AchievementId)));
        }

        if (roleIds?.Count > 0)
        {
            query = query
                .TagWithContext("ByRoles")
                .Where(x => x.Roles.Any(r => roleIds.Contains(r.RoleId)));
        }

        if (userIds?.Count > 0)
        {
            query = query
                .TagWithContext("ByUsers")
                .Where(x => userIds.Contains(x.Id));
        }

        return query
            .Select(x => x.Id)
            .Distinct();
    }
}
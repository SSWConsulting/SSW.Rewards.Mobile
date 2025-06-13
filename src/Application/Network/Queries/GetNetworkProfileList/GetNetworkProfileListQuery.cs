using SSW.Rewards.Application.Common.Exceptions;
using SSW.Rewards.Shared.DTOs.Network;
using SSW.Rewards.Shared.DTOs.Users;

namespace SSW.Rewards.Application.Network.Queries;

public class GetNetworkProfileListQuery : IRequest<NetworkProfileListViewModel>;

/// <summary>
/// This handler has a bit more comments than the usual file as the DB schema
/// require us to do interesting queries to fetch data without overwhelming SQL Server.
/// This handler needs to get all user profiles that:
/// - Has scanned you
/// - You have scanned them
/// - Are staff member
/// 
/// The challenge is that AchievementId are in Users and StaffMembers table loosely
/// associated to UserAchievements table. To make things worse, StaffMembers is loosely
/// associated with Users by Email.
/// </summary>
public class GetNetworkProfileListHandler : IRequestHandler<GetNetworkProfileListQuery, NetworkProfileListViewModel>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IProfilePicStorageProvider _profilePicStorageProvider;

    public GetNetworkProfileListHandler(IApplicationDbContext dbContext, ICurrentUserService currentUserService, IProfilePicStorageProvider profilePicStorageProvider)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _profilePicStorageProvider = profilePicStorageProvider;
    }

    public async Task<NetworkProfileListViewModel> Handle(GetNetworkProfileListQuery request, CancellationToken cancellationToken)
    {
        var userEmail = _currentUserService.GetUserEmail();

        var defaultProfilePictureUrl = await _profilePicStorageProvider.GetProfilePicUri("v2sophie.png");

        var defaultProfilePictureUrlString = defaultProfilePictureUrl != null ? defaultProfilePictureUrl.ToString() : string.Empty;

        // Get user ID, user achievement ID and staff achievement ID.
        // Usually it only has one of the achievement IDs.
        var user = await _dbContext.Users
            .AsNoTracking()
            .TagWithContext("GetUserInfo")
            .Where(x => x.Email == userEmail)
            .Select(x => new
            {
                x.Id,
                x.AchievementId,
                StaffAchievementId = _dbContext.StaffMembers
                    .Where(x => x.Email == userEmail)
                    .Select(x => x.StaffAchievementId)
                    .FirstOrDefault()
            })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("No user found");

        // Get all users for which we have achievements.
        // We need to reverse search from our achievements to other user's achievement IDs.
        var scannedUsersQuery = _dbContext.Users
            .AsNoTracking()
            .TagWithContext("GetScannedUsers")
            .Where(u => u.Activated)
            .Join(
                _dbContext.UserAchievements.AsNoTracking().Where(ua => ua.UserId == user.Id),
                user => user.AchievementId,
                ua => ua.AchievementId,
                (user, ua) => new NetworkProfileDto
                {
                    UserId = user.Id,
                    Email = user.Email ?? string.Empty,
                    Name = user.FullName ?? string.Empty,
                    ProfilePicture = user.Avatar ?? string.Empty,
                    AchievementId = user.AchievementId ?? 0,
                    Scanned = true,
                    ScannedMe = false
                });


        // Get all users that scanned us.
        // We need to filter other users achievements based on current user/staff achievement ID.
        var scannedMeQuery = _dbContext.UserAchievements
            .AsNoTracking()
            .TagWithContext("GetUserScannedMe")
            .Where(x => (x.AchievementId == user.AchievementId || x.AchievementId == user.StaffAchievementId)
                        && x.User.Activated)
            .Select(x => new NetworkProfileDto
            {
                UserId = x.User.Id,
                Email = x.User.Email ?? string.Empty,
                Name = x.User.FullName ?? string.Empty,
                ProfilePicture = x.User.Avatar ?? string.Empty,
                AchievementId = x.User.AchievementId ?? 0,
                Scanned = false,
                ScannedMe = true
            });

        // Merge the 2 queries from DB in 1 go. (a bit more efficient than running them separately)
        var allScannedUsers = await scannedUsersQuery
            .Concat(scannedMeQuery)
            .ToListAsync(cancellationToken);

        // Get all staff members that have achievements and a user profile.
        var staff = await _dbContext.StaffMembers
            .AsNoTracking()
            .TagWithContext("GetAllStaff")
            .Where(x => !x.IsDeleted && x.StaffAchievementId.HasValue)
            .Join(
                _dbContext.Users.AsNoTracking().Where(x => x.Activated),
                user => user.Email,
                staff => staff.Email,
                (s, u) => new NetworkProfileDto
                {
                    UserId = u.Id,
                    Email = u.Email ?? string.Empty,
                    Name = u.FullName ?? string.Empty,
                    ProfilePicture = u.Avatar ?? string.Empty,
                    AchievementId = s.StaffAchievement!.Id,
                    Value = s.StaffAchievement!.Value,
                    IsStaff = true
                })
            .ToListAsync(cancellationToken);

        // Finding scanned staff users and members at the same time from DB is fairly IO intensive.
        // Not only we need to check if we scanned staff member, we also then need to join with
        // their user profile by email and check if they are active.
        // Since we need to fetch all staff, we'll check scans for them in-memory instead.
        var scannedAchievements = await _dbContext.UserAchievements
            .AsNoTracking()
            .TagWithContext("UserAchievementsForStaff")
            .Where(x => x.UserId == user.Id && x.Achievement.Type == AchievementType.Scanned)
            .Select(x => x.AchievementId)
            .ToListAsync(cancellationToken);

        // Merge all scanned users, scanned by and staff members with user info.
        // This consolidates them all and correctly sets scanned, scanned me, is staff, etc.
        var networkList = allScannedUsers
            .Concat(staff)
            .GroupBy(x => new { x.UserId, x.Email, x.Name, x.ProfilePicture })
            .Select(g => new NetworkProfileDto
            {
                UserId = g.Key.UserId,
                Email = g.Key.Email ?? string.Empty,
                Name = g.Key.Name ?? string.Empty,
                ProfilePicture = g.Key.ProfilePicture ?? defaultProfilePictureUrlString,
                AchievementId = g.Max(x => x.AchievementId),
                Scanned = g.Any(x => x.Scanned || scannedAchievements.Contains(x.AchievementId)),
                ScannedMe = g.Any(x => x.ScannedMe),
                IsStaff = g.Any(x => x.IsStaff),
                Value = g.Max(x => x.Value)
            })
            .OrderByDescending(x => x.Value)
            .ToList();

        for (int i = 0; i < networkList.Count; ++i)
        {
            networkList[i].Rank = i + 1;
        }

        return new NetworkProfileListViewModel { Profiles = networkList };
    }
}
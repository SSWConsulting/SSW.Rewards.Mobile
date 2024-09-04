using MoreLinq;
using SSW.Rewards.Shared.DTOs.Network;
using SSW.Rewards.Shared.DTOs.Users;

namespace SSW.Rewards.Application.Network.Queries;

public class GetNetworkProfileListQuery : IRequest<NetworkProfileListViewModel>;

public class GetNetworkProfileListHandler : IRequestHandler<GetNetworkProfileListQuery, NetworkProfileListViewModel>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserService _userService;

    public GetNetworkProfileListHandler(
        IApplicationDbContext dbContext,
        IUserService userService)
    {
        _dbContext = dbContext;
        _userService = userService;
    }

    public async Task<NetworkProfileListViewModel> Handle(GetNetworkProfileListQuery request, CancellationToken cancellationToken)
    {
        var profiles = new List<NetworkProfileDto>();
        var user = await _userService.GetCurrentUser(cancellationToken);
        int currentUserAchievementId;
        
        // 1. Get all staff
        var staff = await _dbContext.StaffMembers
            .Include(u => u.StaffAchievement)
            .Where(s => !s.IsDeleted && s.StaffAchievement != null)
            .ToListAsync(cancellationToken);

        var staffUsers = await _dbContext.Users
            .Where(u => u.Id != user.Id && staff.Select(s => s.Email).Contains(u.Email))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        if (staff.Any(s => s.Email == user.Email))
        {
            var staffProfile = staff.FirstOrDefault(u => u.Email == user.Email);
            
            currentUserAchievementId = staffProfile?.StaffAchievement?.Id ?? -1;
        }
        else
        {
            var userProfile = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Email == user.Email, cancellationToken: cancellationToken);
            
            currentUserAchievementId = userProfile?.AchievementId ?? -1;
        }

        // 2. All users that I have scanned
        var allAchievementIds = await _dbContext.Users
            .Include(u => u.Achievement)
            .Where(u => u.Achievement != null)
            .Select(u => u.Achievement!.Id)
            .Union(staff.Select(s => s.StaffAchievement!.Id))
            .ToListAsync(cancellationToken);

        var scannedUserAchievements = await _dbContext.UserAchievements
            .Where(ua => ua.UserId == user.Id && allAchievementIds.Contains(ua.AchievementId))
            .Include(ua => ua.User)
            .Select(ua => ua.Achievement.Id)
            .ToListAsync(cancellationToken);
        
        foreach (var scannedUserAchievement in scannedUserAchievements)
        {
            User? userMatch;
            var staffMatch = staff.FirstOrDefault(ua => ua.StaffAchievement!.Id == scannedUserAchievement);

            if (staffMatch != null)
            {
                userMatch = staffUsers.FirstOrDefault(u => u.Email == staffMatch.Email);
            }
            else
            {
                userMatch = await _dbContext.Users
                    .Include(u => u.Achievement)
                    .Where(u => u.Achievement != null)
                    .FirstOrDefaultAsync(ua => ua.Achievement!.Id == scannedUserAchievement, cancellationToken: cancellationToken);
            }
            
            if (userMatch != null)
            {
                profiles.Add(new NetworkProfileDto()
                {
                    UserId          = userMatch.Id,
                    Email           = userMatch.Email ?? string.Empty,
                    Name            = userMatch.FullName ?? string.Empty,
                    ProfilePicture  = userMatch.Avatar ?? string.Empty,
                    Scanned         = true
                });
            }
        }

        // 3. All users that have scanned me
        var usersThatHaveScannedMe = await _dbContext.UserAchievements
            .Where(ua => ua.AchievementId == currentUserAchievementId)
            .Include(ua => ua.User)
            .Select(ua => ua.User)
            .ToListAsync(cancellationToken);

        foreach (var scannedMeUser in usersThatHaveScannedMe)
        {
            if (profiles.Any(p => p.Email == scannedMeUser.Email))
            {
                profiles.First(p => p.Email == scannedMeUser.Email).ScannedMe = true;
            }
            else
            {
                profiles.Add(new NetworkProfileDto
                {
                    UserId          = scannedMeUser.Id,
                    Email           = scannedMeUser.Email ?? string.Empty,
                    Name            = scannedMeUser.FullName ?? string.Empty,
                    ProfilePicture  = scannedMeUser.Avatar ?? string.Empty,
                    ScannedMe       = true
                });
            }
        }

        // 4. Add all staff
        foreach (var staffUser in staffUsers)
        {
            if (profiles.Any(p => p.Email == staffUser.Email))
            {
                profiles.First(p => p.Email == staffUser.Email).IsStaff = true;
            }
            else
            {
                profiles.Add(new NetworkProfileDto
                {
                    UserId         = staffUser.Id,
                    Email          = staffUser.Email ?? string.Empty,
                    Name           = staffUser.FullName ?? string.Empty,
                    ProfilePicture = staffUser.Avatar ?? string.Empty,
                    IsStaff        = true
                });
            }
        }

        profiles
            .OrderByDescending(p => p.TotalPoints)
            .ForEach(p => p.Rank = profiles.IndexOf(p) + 1);

        return new NetworkProfileListViewModel { Profiles = profiles };
    }
}

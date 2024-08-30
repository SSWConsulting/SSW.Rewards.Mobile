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

        // 1. All users that I have scanned
        var userAchievementIds = await _dbContext.Users
            .Where(u => u.AchievementId.HasValue)
            .Select(u => u.AchievementId)
            .ToListAsync(cancellationToken);

        var usersIHaveScanned = await _dbContext.UserAchievements
            .Where(ua => userAchievementIds.Contains(ua.AchievementId))
            .Include(ua => ua.User)
            .Select(ua => ua.User)
            .ToListAsync(cancellationToken);

        profiles.AddRange(usersIHaveScanned.Select(u => new NetworkProfileDto
        {
            Email           = u.Email ?? string.Empty,
            Name            = u.FullName ?? string.Empty,
            ProfilePicture  = u.Avatar??string.Empty,
            Scanned         = true
        }));

        // 2. All users that have scanned me
        var usersThatHaveScannedMe = await _dbContext.UserAchievements
            .Where(ua => ua.UserId == user.Id)
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
                    Email           = scannedMeUser.Email ?? string.Empty,
                    Name            = scannedMeUser.FullName ?? string.Empty,
                    ProfilePicture  = scannedMeUser.Avatar ?? string.Empty,
                    ScannedMe       = true
                });
            }
        }

        // 4. All staff
        var staffEmails = await _dbContext.StaffMembers
            .Where(s => !s.IsDeleted)
            .Select(s => s.Email)
            .ToListAsync(cancellationToken);

        var staffUsers = await _dbContext.Users
            .Where(u => staffEmails.Contains(u.Email))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

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

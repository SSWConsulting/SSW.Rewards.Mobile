using MoreLinq;
using SSW.Rewards.Application.Common.Extensions;
using SSW.Rewards.Shared.DTOs.Leaderboard;
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
        var achievements = await _userService.GetUserAchievements(user.Id, cancellationToken);
        
        var completedAchievements = achievements.UserAchievements
            .Where(a => a.Complete)
            .Select(a => a.AchievementId)
            .ToList();

        // TODO: This may not be the best approach, see https://github.com/SSWConsulting/SSW.Rewards.Mobile/issues/759
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
            .Where(x => (!x.IsDeleted && x.Activated) && x.UserId != user.Id)
            .ToListAsync(cancellationToken);
        
        var leaderboardUserDtos = await _dbContext.Users
            .Where(u => u.Activated)
            .Include(u => u.UserAchievements)
            .ThenInclude(ua => ua.Achievement)
            .Where(u => !string.IsNullOrWhiteSpace(u.FullName))
            .Select(u => new LeaderboardUserDto(u, DateTime.Now.FirstDayOfWeek()))
            .ToListAsync(cancellationToken);
            

        leaderboardUserDtos
            .OrderByDescending(lud => lud.TotalPoints)
            .ForEach((u, i) => u.Rank = i + 1);
        
        profiles.AddRange(staffDtos.Select(profile =>
        {
            var leaderboardUser = leaderboardUserDtos.FirstOrDefault(u => u.UserId == profile.UserId);
            
            return new NetworkProfileDto
            {
                UserId = profile.UserId,
                ProfilePicture = profile.ProfilePicture,
                TotalPoints = leaderboardUser.TotalPoints,
                Rank = leaderboardUser.Rank,
                IsExternal = false,
                Name = profile.Name,
                Title = profile.Title,
                Email = profile.Email,
                AchievementId = profile.AchievementId,
                Scanned = (bool)(completedAchievements?.Contains(profile.AchievementId)),
            };
        }));
        
        return new NetworkProfileListViewModel { Profiles = profiles };
    }
}
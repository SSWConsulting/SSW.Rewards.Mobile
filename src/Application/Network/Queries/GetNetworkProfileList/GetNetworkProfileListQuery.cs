using AutoMapper.QueryableExtensions;
using SSW.Rewards.Shared.DTOs.Leaderboard;
using SSW.Rewards.Shared.DTOs.Network;
using SSW.Rewards.Shared.DTOs.Users;

namespace SSW.Rewards.Application.Network.Queries;

public class GetNetworkProfileListQuery : IRequest<NetworkProfileListViewModel>;

public class GetNetworkProfileListHandler : IRequestHandler<GetNetworkProfileListQuery, NetworkProfileListViewModel>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserService _userService;
    private readonly IMapper _mapper;

    public GetNetworkProfileListHandler(
        IApplicationDbContext dbContext, 
        IUserService userService, 
        IMapper mapper)
    {
        _dbContext = dbContext;
        _userService = userService;
        _mapper = mapper;
    }
    
    public async Task<NetworkProfileListViewModel> Handle(GetNetworkProfileListQuery request, CancellationToken cancellationToken)
    {
        var user = await _userService.GetCurrentUser(cancellationToken);
        var achievements = await _userService.GetUserAchievements(user.Id, cancellationToken);
        
        var completedAchievements = achievements.UserAchievements
            .Where(a => a.Complete)
            .Select(a => a.AchievementId)
            .ToList();

        // TODO: This may not be the best approach, see https://github.com/SSWConsulting/SSW.Rewards.Mobile/issues/759
        var staffDtos = await _dbContext.StaffMembers
            .Where(x => !x.IsDeleted && x.Email != user.Email)
            .Join(_dbContext.Users,
                staff => staff.Email, 
                user => user.Email,
                (staff, user) =>
                new NetworkProfileDto
                {
                    UserId = user.Id,
                    Name = staff.Name,
                    ProfilePicture = user.Avatar,
                    Title = staff.Title,
                    Email = staff.Email,
                    AchievementId = staff.StaffAchievement.Id,
                })
            .ToListAsync(cancellationToken);
        
        var users = await _dbContext.Users
            .Where(u => u.Activated)
            .Include(u => u.UserAchievements)
            .ThenInclude(ua => ua.Achievement)
            .ProjectTo<LeaderboardUserDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        // need to set rank outside of AutoMapper
        var leaderboardUserDtos = users
            .Where(u => !string.IsNullOrWhiteSpace(u.Name))
            .OrderByDescending(u => u.TotalPoints)
            .Select((u, i) =>
            {
                u.Rank = i + 1;
                return u;
            }).ToList();
        
        staffDtos = staffDtos
            .Select(profile =>
            {
                var leaderboardUser = leaderboardUserDtos.FirstOrDefault(u => u.UserId == profile.UserId);
                
                profile.TotalPoints = leaderboardUser.TotalPoints;
                profile.Rank = leaderboardUser.Rank;
                profile.Scanned = (bool)(completedAchievements?.Contains(profile.AchievementId));
                profile.IsExternal = false;

                return profile;
            }).ToList();
        
        return new NetworkProfileListViewModel { Profiles = staffDtos };
    }
}
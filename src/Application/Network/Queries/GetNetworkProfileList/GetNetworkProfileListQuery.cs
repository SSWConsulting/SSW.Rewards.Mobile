using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
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
        var profiles = new List<NetworkProfileDto>(200);

        var leaderboardList = _dbContext.Users
            .Where(u => u.Activated && !string.IsNullOrWhiteSpace(u.FullName))
            .Include(u => u.UserAchievements)
            .ThenInclude(ua => ua.Achievement)
            .ProjectTo<LeaderboardUserDto>(_mapper.ConfigurationProvider)
            .OrderByDescending(u => u.TotalPoints)
            .ToList()
            .Select((u, i) =>
            {
                u.Rank = i + 1;
                return u;
            })
            .ToList();
        
        var user = await _userService.GetCurrentUser(cancellationToken);
        var achievements = await _userService.GetUserAchievements(user.Id, cancellationToken);
        
        var networkProfileDtos = await _dbContext.StaffMembers
            .Where(staff => !staff.IsDeleted && staff.Email != user.Email)
            .Join(_dbContext.Users,
                staff => staff.Email, 
                user => user.Email,
                (staff, user) => new NetworkMap { Staff = staff, User = user })
            .ProjectTo<NetworkProfileDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
        
        var completedAchievements = achievements.UserAchievements
            .Where(a => a.Complete)
            .Select(a => a.AchievementId)
            .ToList();
        
        profiles.AddRange(networkProfileDtos.Select(profile =>
        {
            var leaderboardUser = leaderboardList.FirstOrDefault(u => u.UserId == profile.UserId);
            
            profile.TotalPoints = leaderboardUser.TotalPoints;
            profile.Rank = leaderboardUser.Rank;
            profile.Scanned = (bool)(completedAchievements?.Contains(profile.AchievementId));

            return profile;
        }));
        
        return new NetworkProfileListViewModel { Profiles = profiles };
    }
}
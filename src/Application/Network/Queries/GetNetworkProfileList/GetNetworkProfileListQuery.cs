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
        var profiles = new List<NetworkingProfileDto>(200);
        var staffDtos = await _dbContext.StaffMembers
            .Join(_dbContext.Users,
                staff => staff.Email, 
                user => user.Email,
                (staff, user) =>
                new
                    {
                        UserId = user.Id,
                        staff.Name,
                        ProfilePicture = user.Avatar ?? "v2sophie",
                        staff.Title,
                        staff.Email,
                        AchievementId = staff.StaffAchievement.Id,
                        staff.IsDeleted,
                    })
            .Where(x => !x.IsDeleted)
            .ToListAsync(cancellationToken);
        
        var user = await _userService.GetCurrentUser(cancellationToken);
        var achievements = await _userService.GetUserAchievements(user.Id, cancellationToken);

        var completedAchievements = achievements.UserAchievements
            .Where(a => a.Complete)
            .Select(a => a.AchievementId)
            .ToList();

        profiles.AddRange(staffDtos.Select(profile => new NetworkingProfileDto
        {
            UserId = profile.UserId,
            ProfilePicture = profile.ProfilePicture,
            Name = profile.Name,
            Title = profile.Title,
            Email = profile.Email,
            AchievementId = profile.AchievementId,
            Scanned = (bool)(completedAchievements?.Contains(profile.AchievementId))
        }));
        
        return new NetworkProfileListViewModel { Profiles = profiles };
    }
}
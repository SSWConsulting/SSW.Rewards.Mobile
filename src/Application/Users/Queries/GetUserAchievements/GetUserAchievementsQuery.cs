using SSW.Rewards.Application.Users.Common;

namespace SSW.Rewards.Application.Users.Queries.GetUserAchievements;

public class GetUserAchievementsQuery : IRequest<UserAchievementsViewModel>
{
    public int UserId { get; set; }        
}

public class GetUserAchievementsQueryHandler : IRequestHandler<GetUserAchievementsQuery, UserAchievementsViewModel>
{
    private readonly IUserService _userService;

    public GetUserAchievementsQueryHandler(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<UserAchievementsViewModel> Handle(GetUserAchievementsQuery request, CancellationToken cancellationToken)
    {
        return await _userService.GetUserAchievements(request.UserId, cancellationToken);
    }
}

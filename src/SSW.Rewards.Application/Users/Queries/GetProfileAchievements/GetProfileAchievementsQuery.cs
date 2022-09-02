using SSW.Rewards.Application.Users.Common;

namespace SSW.Rewards.Application.Users.Queries.GetProfileAchievements;

public class GetProfileAchivementsQuery : IRequest<UserAchievementsViewModel>
{
    public int UserId { get; set; }
}

public class GetProfileAchievementQueryHandler : IRequestHandler<GetProfileAchivementsQuery, UserAchievementsViewModel>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserService _userService;

    public GetProfileAchievementQueryHandler(IApplicationDbContext context, IUserService userService)
    {
        _context = context;
        _userService = userService;
    }

    public async Task<UserAchievementsViewModel> Handle(GetProfileAchivementsQuery request, CancellationToken cancellationToken)
    {
        var userAchievements = await _userService.GetUserAchievements(request.UserId, cancellationToken);

        var userAchievementNames = userAchievements.UserAchievements.Select(u => u.AchievementName).ToList();

        var achievements = await _context.Achievements
            .Where(a => (a.Type == AchievementType.Completed || a.Type == AchievementType.Linked) && !userAchievementNames.Contains(a.Name))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var profileAchievements = userAchievements.UserAchievements
            .Where(u => u.AchievementType == AchievementType.Linked || u.AchievementType == AchievementType.Completed)
            .ToList();

        foreach (var achievement in achievements)
        {
            profileAchievements.Add(new UserAchievementDto
            {
                AchievementName             = achievement.Name,
                AchievementType             = achievement.Type,
                AchievementValue            = achievement.Value,
                AchievementIcon             = achievement.Icon,
                AchievementIconIsBranded    = achievement.IconIsBranded,
                Complete                    = false,
                AchievementId               = achievement.Id,
            });
        }

        userAchievements.UserAchievements = profileAchievements;

        return userAchievements;
    }
}

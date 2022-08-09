namespace SSW.Rewards.Application.Achievements.Commands.ClaimSocialMediaAchievementForUser;
public class ClaimSocialMediaAchievementForUser : IRequest<int>
{
    public int SocialMediaPlatformId { get; set; }
}

public sealed class ClaimSocialMediaAchievementForUserHandler : IRequestHandler<ClaimSocialMediaAchievementForUser, int>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserService _userService;
    private readonly IDateTime _dateTimeService;

    public ClaimSocialMediaAchievementForUserHandler(IApplicationDbContext context, ICurrentUserService currentUserService, IUserService userService, IDateTime dateTimeService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _userService = userService;
        _dateTimeService = dateTimeService;
    }

    public async Task<int> Handle(ClaimSocialMediaAchievementForUser request, CancellationToken cancellationToken)
    {
        int? achievementId = await _context.SocialMediaPlatforms
                                          .AsNoTracking()
                                          .Where(x => x.Id == request.SocialMediaPlatformId)
                                          .Select(x => x.AchievementId)
                                          .FirstOrDefaultAsync(cancellationToken);
        if (achievementId == null)
            return 0;
        int currentUserId = await _userService.GetUserId(_currentUserService.GetUserEmail());
        var userAchievement = await _context.UserAchievements
                                            .AsNoTracking()
                                            .Where(x => x.UserId == currentUserId && x.AchievementId == achievementId.Value)
                                            .FirstOrDefaultAsync(cancellationToken);
        if (userAchievement == null)
        {
            userAchievement = new UserAchievement
            {
                AchievementId = achievementId.Value,
                UserId        = currentUserId,
                AwardedAt     = _dateTimeService.Now,
                CreatedUtc    = _dateTimeService.Now
            };
            _context.UserAchievements.Add(userAchievement);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return userAchievement.Id;
    }
}


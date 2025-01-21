namespace SSW.Rewards.Application.Achievements.Commands.ClaimSocialMediaAchievementForUser;

public class ClaimSocialMediaAchievementForUserCommand : IRequest<int>
{
    public int SocialMediaPlatformId { get; set; }
}

public sealed class ClaimSocialMediaAchievementForUserCommandHandler : IRequestHandler<ClaimSocialMediaAchievementForUserCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserService _userService;
    private readonly ICacheService _cacheService;
    private readonly IDateTime _dateTimeService;

    public ClaimSocialMediaAchievementForUserCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService, IUserService userService, IDateTime dateTimeService, ICacheService cacheService = null)
    {
        _context = context;
        _currentUserService = currentUserService;
        _userService = userService;
        _dateTimeService = dateTimeService;
        _cacheService = cacheService;
    }

    public async Task<int> Handle(ClaimSocialMediaAchievementForUserCommand request, CancellationToken cancellationToken)
    {
        int? achievementId = await _context.SocialMediaPlatforms
                                          .AsNoTracking()
                                          .Where(x => x.Id == request.SocialMediaPlatformId)
                                          .Select(x => x.AchievementId)
                                          .FirstOrDefaultAsync(cancellationToken);
        if (achievementId == null)
            return 0;
        int currentUserId = await _userService.GetUserId(_currentUserService.GetUserEmail(), cancellationToken);
        var userAchievement = await _context.UserAchievements
                                            .Where(x => x.UserId == currentUserId && x.AchievementId == achievementId.Value)
                                            .FirstOrDefaultAsync(cancellationToken);

        if (userAchievement == null)
        {
            //TECHNICAL DEBT:   I tried to just use the user ID and achievement ID
            //                  for the userAchievement entity, but this resulted
            //                  in a strange bug. Switching to use the user and
            //                  achievement entities resolved the problem. See:
            //                  https://github.com/SSWConsulting/SSW.Rewards.API/issues/20
            var user = await _context.Users.FindAsync(currentUserId, cancellationToken);
            var achievement = await _context.Achievements.FindAsync(achievementId, cancellationToken);

            userAchievement = new UserAchievement
            {
                Achievement = achievement,
                User        = user,
                AwardedAt   = _dateTimeService.Now,
                CreatedUtc  = _dateTimeService.Now
            };
            _context.UserAchievements.Add(userAchievement);
            await _context.SaveChangesAsync(cancellationToken);

            _cacheService.Remove(CacheTags.UpdatedRanking);
        }

        return userAchievement.Id;
    }
}


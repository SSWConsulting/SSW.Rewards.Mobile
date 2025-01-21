using Microsoft.Extensions.Logging;
using SSW.Rewards.Shared.DTOs.Achievements;

namespace SSW.Rewards.Application.Achievements.Command.ClaimAchievementForUser;

public class ClaimAchievementForUserCommand : IRequest<ClaimAchievementResult>
{
    public int UserId { get; set; }
    public string Code { get; set; }
}

public class ClaimAchievementForUserCommandHandler : IRequestHandler<ClaimAchievementForUserCommand, ClaimAchievementResult>
{
    private readonly IApplicationDbContext _context;
    private readonly ICacheService _cacheService;
    private readonly ILogger<ClaimAchievementForUserCommand> _logger;

    public ClaimAchievementForUserCommandHandler(
            IApplicationDbContext context,
            ICacheService cacheService,
            ILogger<ClaimAchievementForUserCommand> logger)
    {
        _context = context;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<ClaimAchievementResult> Handle(ClaimAchievementForUserCommand request, CancellationToken cancellationToken)
    {
        var achievement = await _context
            .Achievements
            .Where(a => a.Code == request.Code)
            .FirstOrDefaultAsync(cancellationToken);

        if (achievement == null)
        {
            _logger.LogError("Achievement was not found for code: {AchievementCode}", request.Code);
            return new ClaimAchievementResult
            {
                status = ClaimAchievementStatus.NotFound
            };
        }

        var user = await _context.Users
            .Where(u => u.Id == request.UserId)
            .Include(u => u.UserAchievements).ThenInclude(ua => ua.Achievement)
            .FirstOrDefaultAsync(cancellationToken);

        if (user == null)
        {
            _logger.LogError("User was not found for id: {UserId}", request.UserId);
            return new ClaimAchievementResult
            {
                status = ClaimAchievementStatus.Error
            };
        }

        var achievementCheck = user
            .UserAchievements
            .Where(ua => ua.Achievement.Code == request.Code)
            .FirstOrDefault();

        if (achievementCheck != null)
        {
            _logger.LogError("User already has achievement: {AchievementCode}", request.Code);
            return new ClaimAchievementResult
            {
                status = ClaimAchievementStatus.Duplicate
            };
        }

        try
        {
            _context.UserAchievements
                .Add(new UserAchievement
                {
                    UserId = user.Id,
                    AchievementId = achievement.Id
                });

            await _context.SaveChangesAsync(cancellationToken);

            _cacheService.Remove(CacheTags.UpdatedRanking);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unable to claim achievement {AchievementId} for User {UserId} with error {ErrorMessage}", achievement.Id, user.Id, e.Message);
            return new ClaimAchievementResult
            {
                status = ClaimAchievementStatus.Error
            };
        }

        return new ClaimAchievementResult
        {
            status = ClaimAchievementStatus.Claimed
        };
    }
}
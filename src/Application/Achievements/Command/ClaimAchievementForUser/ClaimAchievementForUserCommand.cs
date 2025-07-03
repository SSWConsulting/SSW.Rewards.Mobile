using MediatR;
using Microsoft.Extensions.Logging;
using SSW.Rewards.Application.Achievements.Notifications;
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
    private readonly ILogger<ClaimAchievementForUserCommand> _logger;
    private readonly IMediator _mediator;

    public ClaimAchievementForUserCommandHandler(
            IApplicationDbContext context,
            ILogger<ClaimAchievementForUserCommand> logger,
            IMediator mediator)
    {
        _context = context;
        _logger = logger;
        _mediator = mediator;
    }

    public async Task<ClaimAchievementResult> Handle(ClaimAchievementForUserCommand request, CancellationToken cancellationToken)
    {
        var achievement = await _context.Achievements
            .AsNoTracking()
            .TagWithContext("ClaimAchievementForUser")
            .Where(a => a.Code == request.Code)
            .Select(x => new { x.Id })
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
            .AsNoTracking()
            .TagWithContext("GetUserId")
            .Where(u => u.Id == request.UserId)
            .Select(x => new { x.Id })
            .FirstOrDefaultAsync(cancellationToken);

        if (user == null)
        {
            _logger.LogError("User was not found for id: {UserId}", request.UserId);
            return new ClaimAchievementResult
            {
                status = ClaimAchievementStatus.Error
            };
        }

        var hasAchievement = await _context.UserAchievements
            .AsNoTracking()
            .TagWithContext("HasAchievement")
            .AnyAsync(x => x.UserId == user.Id && x.Achievement.Code == request.Code, cancellationToken);

        if (hasAchievement)
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

            // Check for any other milestone achievements that the user may have reached.
            await _mediator.Publish(new UserMilestoneAchievementCheckRequested { UserId = user.Id }, cancellationToken);
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
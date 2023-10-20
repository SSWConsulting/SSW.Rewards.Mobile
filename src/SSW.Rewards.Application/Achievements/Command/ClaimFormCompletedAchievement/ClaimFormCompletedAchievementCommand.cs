namespace SSW.Rewards.Application.Achievements.Command.ClaimFormCompletedAchievement;

public class ClaimFormCompletedAchievementCommand : IRequest
{
    public string Email { get; set; }

    public string IntegrationId { get; set; }
}

public class ClaimFormCompletedAchievementCommandHandler : IRequestHandler<ClaimFormCompletedAchievementCommand>
{
    private readonly IApplicationDbContext _dbContext;

    public ClaimFormCompletedAchievementCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<Unit> Handle(ClaimFormCompletedAchievementCommand request, CancellationToken cancellationToken)
    {
        var achievement = await _dbContext.Achievements.FirstOrDefaultAsync(a => a.IntegrationId == request.IntegrationId);

        if (achievement is not null)
        {
            var user = await _dbContext.Users
                .Include(u => u.UserAchievements)
                .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());

            if (user is not null)
            {
                if (!user.UserAchievements.Any(ua => ua.AchievementId == achievement.Id))
                {
                    user.UserAchievements.Add(new UserAchievement
                    {
                        AchievementId = achievement.Id,
                        AwardedAt = DateTime.UtcNow,
                    });
                }
            }
            else
            {
                if (!_dbContext.UnclaimedAchievements.Any(ua => ua.EmailAddress.ToLower() == request.Email.ToLower() && ua.AchievementId == achievement.Id))
                {
                    _dbContext.UnclaimedAchievements.Add(new UnclaimedAchievement
                    {
                        AchievementId = achievement.Id,
                        EmailAddress = request.Email,
                    });
                }
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return Unit.Value;
    }
}

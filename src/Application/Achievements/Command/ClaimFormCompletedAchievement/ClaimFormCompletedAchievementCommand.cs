using SSW.Rewards.Application.Common.Models;

namespace SSW.Rewards.Application.Achievements.Command.ClaimFormCompletedAchievement;

public class ClaimFormCompletedAchievementCommand : IRequest
{
    public string Email { get; set; }
    public string Name { get; set; }
    public string IntegrationId { get; set; }
}

public class ClaimFormCompletedAchievementCommandHandler : IRequestHandler<ClaimFormCompletedAchievementCommand>
{
    private readonly IEmailService _emailService;
    private readonly IApplicationDbContext _dbContext;

    public ClaimFormCompletedAchievementCommandHandler(IApplicationDbContext dbContext, IEmailService emailService)
    {
        _dbContext = dbContext;
        _emailService = emailService;
    }
    
    public async Task Handle(ClaimFormCompletedAchievementCommand request, CancellationToken cancellationToken)
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
                    await _emailService.SendFormCompletionPointsReceivedEmail(user.Email, new FormCompletionPointsReceivedEmail { Points = achievement.Value, UserName = user.FullName }, cancellationToken);
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

                    await _emailService.SendFormCompletionCreateAccountEmail(request.Email.ToLower(), new FormCompletionCreateAccountEmail { Name = request.Name, Points = achievement.Value},
                        cancellationToken);
                }
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}

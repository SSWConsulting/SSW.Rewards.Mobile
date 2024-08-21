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
        var achievement = await _dbContext.Achievements.FirstOrDefaultAsync(a => a.IntegrationId == request.IntegrationId, cancellationToken: cancellationToken);

        if (achievement is not null)
        {
            var user = await _dbContext.Users
                .Include(u => u.UserAchievements)
                .FirstOrDefaultAsync(u => u.Email != null && u.Email.Equals(request.Email, StringComparison.CurrentCultureIgnoreCase), cancellationToken: cancellationToken);

            if (user is not null)
            {
                if (!user.UserAchievements.Any(ua => ua.AchievementId == achievement.Id))
                {
                    user.UserAchievements.Add(new UserAchievement
                    {
                        AchievementId = achievement.Id,
                        AwardedAt = DateTime.UtcNow,
                    
                    });
                    
                    var firstName = user.FullName?.Split([' '], 2).FirstOrDefault();
                    
                    await _emailService.SendFormCompletionPointsReceivedEmail(user.Email!,
                        new FormCompletionPointsReceivedEmail
                        {
                            Points = achievement.Value,
                            FirstName = firstName,
                            AchievementName = achievement.Name
                        }, cancellationToken);
                }
            }
            else
            {
                if (!_dbContext.UnclaimedAchievements.Any(ua => ua.EmailAddress.Equals(request.Email, StringComparison.CurrentCultureIgnoreCase) && ua.AchievementId == achievement.Id))
                {
                    _dbContext.UnclaimedAchievements.Add(new UnclaimedAchievement
                    {
                        AchievementId = achievement.Id,
                        EmailAddress = request.Email
                    });
                    
                    var firstName = request.Name.Split([' '], 2).FirstOrDefault();

                    await _emailService.SendFormCompletionCreateAccountEmail(request.Email.ToLower(),
                        new FormCompletionCreateAccountEmail
                        {
                            Points = achievement.Value,
                            FirstName = firstName,
                            AchievementName = achievement.Name
                        },
                        cancellationToken);
                }
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}

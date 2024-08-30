using SSW.Rewards.Application.Common.Extensions;

namespace SSW.Rewards.Application.System.Commands.SeedUserAchievements;

public class SeedUserAchievementsCommand : IRequest
{

}

public class SeedUserAchievementsCommandHandler(IApplicationDbContext dbContext) : IRequestHandler<SeedUserAchievementsCommand>
{
    public async Task Handle(SeedUserAchievementsCommand request, CancellationToken cancellationToken)
    {
        var nonStaffUsersWithNoAchievement = await dbContext.Users
            .Where(u => !u.Email.ToLower().Contains("ssw.com.au") && u.AchievementId == null)
            .ToListAsync(cancellationToken);

        foreach (var user in nonStaffUsersWithNoAchievement)
        {
            user.GenerateAchievement();
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

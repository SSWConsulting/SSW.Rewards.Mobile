using System.Text;

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
            var codeData = Encoding.ASCII.GetBytes($"ach:{Guid.NewGuid().ToString()}");
            var code = Convert.ToBase64String(codeData);

            var achievement = new Achievement
            {
                Name                = user.FullName,
                Value               = 100,
                Type                = AchievementType.Scanned,
                IsMultiscanEnabled  = false,
                Code                = code
            };

            user.Achievement = achievement;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

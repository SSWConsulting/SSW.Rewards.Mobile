using System.Text;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SSW.Rewards.Application.Achievements.Queries.GetAchievementAdminList;
using SSW.Rewards.Application.Common.Interfaces;

namespace SSW.Rewards.Application.Achievements.Command.PostAchievement;

public class CreateAchievementCommand : IRequest<AchievementAdminViewModel>
{
    public string Name { get; set; }
    public int Value { get; set; }
    public AchievementType Type { get; set; }
}

public class CreateAchievementCommandHandler : IRequestHandler<CreateAchievementCommand, AchievementAdminViewModel>
{
    private readonly IApplicationDbContext _context;

    public CreateAchievementCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AchievementAdminViewModel> Handle(CreateAchievementCommand request, CancellationToken cancellationToken)
    {
        var existingAchievements = await _context.Achievements.ToListAsync(cancellationToken);

        var achievement = existingAchievements
            .FirstOrDefault(a => a.Name.Equals(request.Name, StringComparison.InvariantCulture))
            ?? new Domain.Entities.Achievement();

        achievement.Name = request.Name;
        var codeData = Encoding.ASCII.GetBytes($"ach:{request.Name}");
        achievement.Code = Convert.ToBase64String(codeData);
        achievement.Value = request.Value;
        achievement.Type = request.Type;

        if (achievement.Id == 0)
        {
            _context.Achievements.Add(achievement);
            await _context.SaveChangesAsync(cancellationToken);
            return new AchievementAdminViewModel()
            {
                Code = achievement.Code,
                Name = achievement.Name,
                Value = achievement.Value,
                Type = achievement.Type,
            };

        }

        return null;
    }
}

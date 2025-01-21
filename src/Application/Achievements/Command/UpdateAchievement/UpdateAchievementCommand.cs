using SSW.Rewards.Application.Common.Exceptions;

namespace SSW.Rewards.Application.Achievements.Command.UpdateAchievement;

public class UpdateAchievementCommand : IRequest<Unit>
{
    public int Id { get; set; }
    public int Value { get; set; }
    public AchievementType Type { get; set; }
    public bool IsMultiscanEnabled { get; set; }
}

public sealed class UpdateAchievementCommandHandler : IRequestHandler<UpdateAchievementCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICacheService _cacheService;

    public UpdateAchievementCommandHandler(IApplicationDbContext context, ICacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
    }

    public async Task<Unit> Handle(UpdateAchievementCommand request, CancellationToken cancellationToken)
    {
        var achievement = await _context.Achievements
                                .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

        if (achievement == null)
        {
            throw new NotFoundException(nameof(UpdateAchievementCommandHandler), request.Id);
        }

        achievement.Value = request.Value;
        achievement.Type = request.Type;
        achievement.IsMultiscanEnabled = request.IsMultiscanEnabled;

        await _context.SaveChangesAsync(cancellationToken);

        _cacheService.Remove(CacheTags.UpdatedRanking);

        return Unit.Value;
    }
}

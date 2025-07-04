using SSW.Rewards.Application.Common.Exceptions;

namespace SSW.Rewards.Application.Achievements.Command.DeleteAchievement;

public class DeleteAchievementCommand : IRequest
{
    public int Id { get; set; }
}

public class DeleteAchievementCommandHandler : IRequestHandler<DeleteAchievementCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteAchievementCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteAchievementCommand request, CancellationToken cancellationToken)
    {
        var achievement = await _context.Achievements
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(DeleteAchievementCommand), request.Id);

        // Soft delete the achievement.
        _context.Achievements.Remove(achievement);

        await _context.SaveChangesAsync(cancellationToken);
    }
}

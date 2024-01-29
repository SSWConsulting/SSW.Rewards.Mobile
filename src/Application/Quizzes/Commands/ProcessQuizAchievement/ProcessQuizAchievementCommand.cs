namespace SSW.Rewards.Application.Quizzes.Commands.ProcessQuizAchievement;
public class ProcessQuizAchievementCommand : IRequest<Unit>
{
    public int SubmissionId { get; set; }
}


public sealed class Handler : IRequestHandler<ProcessQuizAchievementCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserService _userService;

    public Handler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IUserService userService
        )
    {
        _context = context;
        _currentUserService = currentUserService;
        _userService = userService;
    }

    public async Task<Unit> Handle(ProcessQuizAchievementCommand request, CancellationToken cancellationToken)
    {
        // get the quiz
        Quiz dbQuiz = await _context.CompletedQuizzes
            .Where(x => 
                x.Id == request.SubmissionId)
            .Select(x => x.Quiz)
            .FirstAsync(cancellationToken);

        // see if the user already has the achievement for this quiz
        // NOTE: This exists here (rather than in a Validator) because we don't want to
        // throw an exception if the user already has the achievement
        int userId = await _userService.GetUserId(_currentUserService.GetUserEmail(), cancellationToken);
        bool hasAchievement = await _context.UserAchievements
            .Where(x =>
                    x.UserId == userId
                &&  x.Id == dbQuiz.AchievementId)
            .AnyAsync(cancellationToken);

        if (hasAchievement)
            return Unit.Value;

        // create the achievement
        var achievement = new UserAchievement
        {
            UserId          = userId,
            AchievementId   = dbQuiz.AchievementId,
            AwardedAt       = DateTime.Now,
        };
        await _context.UserAchievements.AddAsync(achievement);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
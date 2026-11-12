namespace SSW.Rewards.Application.Quizzes.Commands.SubmitUserQuiz;

public class SubmitUserQuizCommandValidator : AbstractValidator<SubmitUserQuizCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserService _userService;

    public SubmitUserQuizCommandValidator(IApplicationDbContext context, ICurrentUserService currentUserService, IUserService userService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _userService = userService;

        RuleFor(x => x.QuizId)
            .MustAsync(CanSubmit)
            .WithMessage("You have already passed this quiz");
    }

    public async Task<bool> CanSubmit(int quizId, CancellationToken token)
    {
        int userId = await _userService.GetUserId(_currentUserService.GetUserEmail(), token);

        bool alreadyPassed = await _context.CompletedQuizzes
            .AsNoTracking()
            .TagWithContext()
            .AnyAsync(c => c.QuizId == quizId && c.UserId == userId && c.Passed, token);

        return !alreadyPassed;
    }
}

namespace SSW.Rewards.Application.Quizzes.Commands.SubmitUserQuiz;

public class SubmitUserQuizCommandValidator : AbstractValidator<SubmitUserQuizCommand>
{
    private readonly IApplicationDbContext _context;

    public SubmitUserQuizCommandValidator(IApplicationDbContext context)
    {
        _context = context;
        RuleFor(x => x.QuizId)
            .MustAsync(CanSubmit);
    }

    public async Task<bool> CanSubmit(int quizId, CancellationToken token)
    {
        if (await _context.CompletedQuizzes.AnyAsync(c => c.QuizId == quizId && c.Passed, token))
            return false;

        return true;
    }
}

namespace SSW.Rewards.Application.Quizzes.Commands.AddNewQuiz;
public class AddNewQuizCommandValidator : AbstractValidator<AddNewQuizCommand>
{
    private readonly IApplicationDbContext _context;

    public AddNewQuizCommandValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(c => c)
            .MustAsync(BeUniqueQuiz);
    }

    private async Task<bool> BeUniqueQuiz(AddNewQuizCommand command, CancellationToken cancellationToken)
    {
        return await _context.Quizzes
            .AnyAsync(q => 
                q.Id == command.Quiz.QuizId
                ||
                q.Title.ToLower() == command.Quiz.Title.ToLower(), cancellationToken);
    }
}

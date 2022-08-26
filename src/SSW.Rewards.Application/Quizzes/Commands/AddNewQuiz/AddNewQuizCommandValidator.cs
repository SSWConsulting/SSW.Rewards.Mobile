using SSW.Rewards.Application.Quizzes.Common;

namespace SSW.Rewards.Application.Quizzes.Commands.AddNewQuiz;
public class AddNewQuizCommandValidator : AbstractValidator<AddNewQuizCommand>
{
    private readonly IApplicationDbContext _context;

    public AddNewQuizCommandValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(c => c)
            .MustAsync(BeUniqueQuiz);

        RuleFor(c => c.Quiz.Questions)
            .NotEmpty();

        RuleForEach(c => c.Quiz.Questions)
            .ChildRules(question =>
            {
                question.RuleFor(q => q.Text)
                .NotEmpty();

                question.RuleFor(q => q.Answers)
                .NotEmpty();

                question.RuleFor(q => q)
                .Must(HaveOneCorrectAnswer);
            });
    }

    private async Task<bool> BeUniqueQuiz(AddNewQuizCommand command, CancellationToken cancellationToken)
    {
        return await _context.Quizzes
            .AnyAsync(q => !q.IsArchived && q.Title.ToLower() == command.Quiz.Title.ToLower(), cancellationToken);
    }

    private bool HaveOneCorrectAnswer(QuizQuestionDto question)
    {
        return question.Answers.Where(a => a.IsCorrect).Count() == 1;
    }
}

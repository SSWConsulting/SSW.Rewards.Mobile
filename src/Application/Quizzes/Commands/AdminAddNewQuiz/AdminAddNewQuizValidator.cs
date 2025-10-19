using SSW.Rewards.Shared.DTOs.Quizzes;

namespace SSW.Rewards.Application.Quizzes.Commands.AdminAddNewQuiz;
public class AdminAddNewQuizValidator : AbstractValidator<AdminAddNewQuiz>
{
    private readonly IApplicationDbContext _context;

    public AdminAddNewQuizValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(c => c)
            .MustAsync(BeUniqueQuiz)
            .WithMessage("Not unique");

        RuleFor(q => q.NewQuiz.Title)
            .NotEmpty()
            .WithMessage("Missing title");

        RuleFor(q => q.NewQuiz.Description)
             .NotEmpty()
             .WithMessage("Missing description");

        RuleFor(c => c.NewQuiz.Questions)
            .NotEmpty()
            .WithMessage("No questions");

        RuleForEach(c => c.NewQuiz.Questions)
            .ChildRules(question =>
            {
                question.RuleFor(q => q.Text)
                .NotEmpty()
                .WithMessage("No question text");

                question.RuleFor(q => q.Answers)
                .NotEmpty()
                .WithMessage("No answers");

                question.RuleFor(q => q)
                .Must(HaveOneCorrectAnswer)
                .WithMessage(">1 correct answer");
            });
    }

    private async Task<bool> BeUniqueQuiz(AdminAddNewQuiz command, CancellationToken cancellationToken)
    {
        return !await _context.Quizzes
            .AnyAsync(q => !q.IsArchived && q.Title.ToLower() == command.NewQuiz.Title.ToLower(), cancellationToken);
    }

    private bool HaveOneCorrectAnswer(QuizQuestionEditDto question)
    {
        return question.Answers.Where(a => a.IsCorrect).Count() == 1;
    }
}

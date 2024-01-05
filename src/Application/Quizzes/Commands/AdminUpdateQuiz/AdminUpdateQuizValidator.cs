using Shared.DTOs.Quizzes;

namespace SSW.Rewards.Application.Quizzes.Commands.AddNewQuiz;
public class AdminUpdateQuizValidator : AbstractValidator<AdminUpdateQuiz>
{
    private readonly IApplicationDbContext _context;

    public AdminUpdateQuizValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(c => c)
            .MustAsync(BeUniqueQuiz)
            .WithMessage("Not unique");

        RuleFor(q => q.Quiz.Title)
            .NotEmpty()
            .WithMessage("Missing title");

        RuleFor(q => q.Quiz.Description)
             .NotEmpty()
             .WithMessage("Missing description");

        RuleFor(c => c.Quiz.Questions)
            .NotEmpty()
            .WithMessage("No questions");

        RuleForEach(c => c.Quiz.Questions)
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

    private async Task<bool> BeUniqueQuiz(AdminUpdateQuiz command, CancellationToken cancellationToken)
    {
        return !await _context.Quizzes
            .AnyAsync(q => 
            !q.IsArchived 
            && q.Title.ToLower() == command.Quiz.Title.ToLower()
            && q.Id != command.Quiz.QuizId, cancellationToken);
    }

    private bool HaveOneCorrectAnswer(QuizQuestionDto question)
    {
        return question.Answers.Where(a => a.IsCorrect).Count() == 1;
    }
}

namespace SSW.Rewards.Application.Quizzes.Commands.SubmitAnswerCommand;

public class SubmitAnswerCommandValidator : AbstractValidator<SubmitAnswerCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserService _userService;

    public SubmitAnswerCommandValidator(IApplicationDbContext context, ICurrentUserService currentUserService, IUserService userService)
    {
        _context            = context;
        _currentUserService = currentUserService;
        _userService        = userService;

        RuleFor(c => c)
            .NotNull();
        RuleFor(c => c.AnswerText)
            .NotEmpty()
            .WithMessage("Missing answer text");
        RuleFor(c => c.QuestionId)
            .GreaterThan(0)
            .WithMessage("Missing question id");
        RuleFor(c => c.SubmissionId)
            .GreaterThan(0)
            .WithMessage("Missing submission id");
        RuleFor(c => c)
            .MustAsync(SubmissionExists)
            .WithMessage("No submission with that id found for the current user");
        RuleFor(c => c)
            .MustAsync(BeUnanswered)
            .WithMessage("Question has already been attempted for this submission");
    }

    private async Task<bool> BeUnanswered(SubmitAnswerCommand command, CancellationToken cancellationToken)
    {
        int userId = await _userService.GetUserId(_currentUserService.GetUserEmail(), cancellationToken);
        
        bool alreadyAnswered = await _context.SubmittedAnswers
            .Where(a =>
                        a.SubmissionId == command.SubmissionId
                    &&  a.Submission.UserId == userId
                    &&  a.QuizQuestionId == command.QuestionId)
            .AnyAsync(cancellationToken);
        return !alreadyAnswered;
    }

    private async Task<bool> SubmissionExists(SubmitAnswerCommand command, CancellationToken cancellationToken)
    {
        int userId = await _userService.GetUserId(_currentUserService.GetUserEmail(), cancellationToken);
        bool submissionExists = await _context.CompletedQuizzes
            .Where(s => s.Id == command.SubmissionId && s.UserId == userId)
            .AnyAsync(cancellationToken);
        return submissionExists;
    }
}
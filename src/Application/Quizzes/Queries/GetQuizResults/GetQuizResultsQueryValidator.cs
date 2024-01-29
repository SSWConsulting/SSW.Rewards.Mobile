using SSW.Rewards.Application.Quizzes.Commands.SubmitAnswerCommand;

namespace SSW.Rewards.Application.Quizzes.Queries.GetQuizResults;

public class GetQuizResultsQueryValidator : AbstractValidator<GetQuizResultsQuery>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserService _userService;

    public GetQuizResultsQueryValidator(IApplicationDbContext context, ICurrentUserService currentUserService, IUserService userService)
    {
        _context            = context;
        _currentUserService = currentUserService;
        _userService        = userService;

        RuleFor(c => c)
            .NotNull();
        RuleFor(c => c.SubmissionId)
            .GreaterThan(0)
            .WithMessage("Missing submission id");
        RuleFor(c => c)
            .MustAsync(SubmissionExists)
            .WithMessage("No submission with that id found for the current user");
        RuleFor(c => c)
            .MustAsync(BeFinished)
            .WithMessage("Submission is incomplete");
    }

    
    private async Task<bool> BeFinished(GetQuizResultsQuery query, CancellationToken cancellationToken)
    {
        CompletedQuiz dbCompletedQuiz = await _context.CompletedQuizzes
                .Include(cq => cq.Quiz)
                    .ThenInclude(q => q.Questions)
                .Include(cq => cq.Answers)
                .Where(cq => cq.Id == query.SubmissionId)
                .FirstAsync(cancellationToken);

        return dbCompletedQuiz.Answers.Count == dbCompletedQuiz.Quiz.Questions.Count;
    }
    private async Task<bool> SubmissionExists(GetQuizResultsQuery query, CancellationToken cancellationToken)
    {
        int userId = await _userService.GetUserId(_currentUserService.GetUserEmail(), cancellationToken);
        bool submissionExists = await _context.CompletedQuizzes
            .Where(s =>
                    s.Id == query.SubmissionId
                &&  s.UserId == userId)
            .AnyAsync(cancellationToken);
        return submissionExists;
    }
}
namespace SSW.Rewards.Application.Quizzes.Queries.CheckQuizCompletion;

public class CheckQuizCompletionQuery : IRequest<bool>
{
    public int SubmissionId { get; set; }
}

public sealed class Handler : IRequestHandler<CheckQuizCompletionQuery, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserService _userService;
    public Handler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IUserService userService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _userService = userService;
    }

    public async Task<bool> Handle(CheckQuizCompletionQuery request, CancellationToken cancellationToken)
    {
        int userId = await _userService.GetUserId(_currentUserService.GetUserEmail(), cancellationToken);

        // Ugly query because we need all this data to figure stuff out
        var dbQuiz = await _context.CompletedQuizzes
            .Include(q => q.Quiz)
                .ThenInclude(qu => qu.Questions)
            .Include(q => q.Answers)
                .ThenInclude(a => a.QuizQuestion)
            .Where(q =>
                    q.UserId == userId
                && q.Id == request.SubmissionId)
            .FirstOrDefaultAsync(cancellationToken);

        if (dbQuiz == null)
            throw new ArgumentNullException($"No quiz found for submission id {request.SubmissionId}");

        // have all questions been answered?
        int questionCount = dbQuiz.Quiz.Questions.Count;
        int answerCount = dbQuiz.Answers.Count;

        return questionCount == answerCount;
    }
}

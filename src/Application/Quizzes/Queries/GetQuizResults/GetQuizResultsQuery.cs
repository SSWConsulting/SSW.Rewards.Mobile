using SSW.Rewards.Shared.DTOs.Quizzes;

namespace SSW.Rewards.Application.Quizzes.Queries.GetQuizResults;

public class GetQuizResultsQuery : IRequest<QuizResultDto>
{
    public int SubmissionId { get; set; }
}

public sealed class Handler : IRequestHandler<GetQuizResultsQuery, QuizResultDto>
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

    public async Task<QuizResultDto> Handle(GetQuizResultsQuery request, CancellationToken cancellationToken)
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
                &&  q.Id == request.SubmissionId)
            .FirstOrDefaultAsync(cancellationToken);

        if (dbQuiz == null)
            throw new ArgumentNullException($"No quiz found for submission id {request.SubmissionId}");

        // have all questions been answered?
        int questionCount = dbQuiz.Quiz.Questions.Count;
        int answerCount = dbQuiz.Answers.Count;
        if (questionCount != answerCount) // TODO: Figure out how we want this scenario to be handled
            throw new ArgumentException($"Not all questions have been answered for submission id {request.SubmissionId}");

        // all correct?
        bool passed = dbQuiz.Answers.All(a => a.Correct);

        // update the db record
        dbQuiz.Passed = passed;
        _context.CompletedQuizzes.Update(dbQuiz);
        await _context.SaveChangesAsync(cancellationToken);

        QuizResultDto result = new QuizResultDto
        {
            SubmissionId    = dbQuiz.Id,
            QuizId          = dbQuiz.QuizId,
            Passed          = passed,
            Results         = dbQuiz.Answers.Select(a => new QuestionResultDto
            {
                QuestionId      = a.QuizQuestionId,
                QuestionText    = a.QuizQuestion.Text ?? string.Empty,
                AnswerText      = a.AnswerText,
                ExplanationText = a.GPTExplanation,
                Correct         = a.Correct,
                Confidence      = a.GPTConfidence
            }).ToList()
        };

        return result;
    }
}

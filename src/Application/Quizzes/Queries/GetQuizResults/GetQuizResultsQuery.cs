using System.Linq.Expressions;
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
        _context            = context;
        _currentUserService = currentUserService;
        _userService        = userService;
    }

    public async Task<QuizResultDto> Handle(GetQuizResultsQuery request, CancellationToken cancellationToken)
    {
        int userId = await _userService.GetUserId(_currentUserService.GetUserEmail(), cancellationToken);

        QuizResultDto result = await _context.CompletedQuizzes
            .Where(q =>
                    q.UserId == userId
                &&  q.Id == request.SubmissionId)
            .Select(MapQuizResultDto())
            .FirstAsync(cancellationToken);

        return result;
    }

    private static Expression<Func<CompletedQuiz, QuizResultDto>> MapQuizResultDto()
    {
        return cq => new QuizResultDto
        {
            SubmissionId    = cq.Id,
            QuizId          = cq.QuizId,
            Passed          = cq.Passed,
            Results         = cq.Answers.Select(a =>
                            new QuestionResultDto
                            {
                                QuestionId      = a.QuizQuestionId ?? 0,
                                QuestionText    = a.QuizQuestion!.Text ?? string.Empty,
                                AnswerText      = a.AnswerText,
                                ExplanationText = a.GPTExplanation,
                                Correct         = a.Correct,
                                Confidence      = a.GPTConfidence
                            }).ToList()
        };
    }
}

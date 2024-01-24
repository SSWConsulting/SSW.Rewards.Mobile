using SSW.Rewards.Shared.DTOs.Quizzes;

namespace SSW.Rewards.Application.Quizzes.Queries.GetQuizQuestionsBySubmissionId;

public class GetQuizQuestionsBySubmissionIdQuery : IRequest<List<QuizQuestionDto>>
{
    public int SubmissionId { get; set; }
}

public sealed class Handler : IRequestHandler<GetQuizQuestionsBySubmissionIdQuery, List<QuizQuestionDto>>
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

    public async Task<List<QuizQuestionDto>> Handle(GetQuizQuestionsBySubmissionIdQuery request, CancellationToken cancellationToken)
    {
        int userId = await _userService.GetUserId(_currentUserService.GetUserEmail(), cancellationToken);

        IDictionary<int,string?> questions = await _context.CompletedQuizzes
            .Where(q => 
                    q.UserId == userId 
                &&  q.Id == request.SubmissionId)
            .SelectMany(cq => cq.Quiz.Questions)
            .ToDictionaryAsync(q => q.Id, q => q.Text, cancellationToken);

        List<QuizQuestionDto> result = questions.Select(q => new QuizQuestionDto { QuestionId = q.Key, Text = q.Value ?? "" }).ToList();

        return result ?? throw new ArgumentNullException($"No questions found for submission id { request.SubmissionId }");
    }
}

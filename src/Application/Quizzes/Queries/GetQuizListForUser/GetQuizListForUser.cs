using SSW.Rewards.Shared.DTOs.Quizzes;

namespace SSW.Rewards.Application.Quizzes.Queries.GetQuizListForUser;

public class GetQuizListForUser : IRequest<IEnumerable<QuizDto>> {}

public sealed class Handler : IRequestHandler<GetQuizListForUser, IEnumerable<QuizDto>>
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

    public async Task<IEnumerable<QuizDto>> Handle(GetQuizListForUser request, CancellationToken cancellationToken)
    {
        // get all quizzes
        var masterQuizList = await _context.Quizzes
                                            .Where(q => !q.IsArchived)
                                            .Include(q => q.Questions)
                                                .ThenInclude(x => x.Answers)
                                            .OrderBy(q => q.Title)
                                            .AsNoTracking()
                                            .ToListAsync(cancellationToken);

        // get the quiz Ids for the quizzes the user has completed so we can mark off 
        // the quizzes that they've already completed
        var userId = await _userService.GetUserId(_currentUserService.GetUserEmail(), cancellationToken);
        var userQuizzes = await _context.CompletedQuizzes
                                        .Where(q => q.UserId == userId && q.Passed)
                                        .Select(r => r.QuizId)
                                        .ToListAsync(cancellationToken);

        List<QuizDto> retVal = new List<QuizDto>();
        foreach (var quiz in masterQuizList)
        {
            retVal.Add(new QuizDto
            {
                Id = quiz.Id,
                Title = quiz.Title,
                Description = quiz.Description,
                Icon = quiz.Icon,
                Passed = userQuizzes.Contains(quiz.Id)
            });
        }

        return retVal;
    }
}

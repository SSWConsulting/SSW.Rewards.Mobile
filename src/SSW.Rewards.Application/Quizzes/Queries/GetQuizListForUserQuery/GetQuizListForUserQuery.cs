namespace SSW.Rewards.Application.Quizzes.Queries.GetQuizListForUserQuery;

public class GetQuizListForUserQuery : IRequest<IEnumerable<QuizDto>>
{

    public sealed class Handler : IRequestHandler<GetQuizListForUserQuery, IEnumerable<QuizDto>>
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

        public async Task<IEnumerable<QuizDto>> Handle(GetQuizListForUserQuery request, CancellationToken cancellationToken)
        {
            // get all quizzes
            var masterQuizList = await _context.Quizzes
                                                .Include(q => q.Questions)
                                                    .ThenInclude(x => x.Answers)
                                                .OrderBy(q => q.Title)
                                                .AsNoTracking()
                                                .ToListAsync(cancellationToken);

            // get the quiz Ids for the quizzes the user has completed so we can mark off 
            // the quizzes that they've already completed
            var userId = await _userService.GetUserId(_currentUserService.GetUserEmail());
            var userQuizzes = await _context.CompletedQuizzes
                                            .Where(q => q.UserId == userId)
                                            .Select(r => r.QuizId)
                                            .ToListAsync(cancellationToken);

            List<QuizDto> retVal = new List<QuizDto>();
            foreach(var quiz in masterQuizList)
            {
                retVal.Add(new QuizDto
                {
                    Id          = quiz.Id,
                    Title       = quiz.Title,
                    Description = quiz.Description,
                    Icon        = quiz.Icon,
                    Passed      = userQuizzes.Contains(quiz.Id)
                });
            }

            return retVal;
        }
    }

}
